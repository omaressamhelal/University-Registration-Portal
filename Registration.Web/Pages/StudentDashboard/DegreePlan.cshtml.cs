using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Registration.Web.Pages.StudentDashboard
{
    public class DegreePlanModel : PageModel
    {
        private readonly string _connectionString;

        public DegreePlanModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [BindProperty(SupportsGet = true)]
        public int StudentId { get; set; }

        public StudentDetailsView StudentProfile { get; set; } = new();
        public List<DegreePlanItem> DegreeCourses { get; set; } = new();

        public int TotalRequiredCredits { get; set; } = 155;
        public int TotalEarnedCredits { get; set; } = 0;

        public IActionResult OnGet()
        {
            // =========================================================
            // 🛡️ BULLETPROOF SECURITY: Read ID directly from the Cookie
            // =========================================================
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdString = User.FindFirst("UserId")?.Value;

            if (userRole == "Student")
            {
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int cookieId) && cookieId > 0)
                {
                    StudentId = cookieId;
                }
                else
                {
                    return RedirectToPage("/Login/Index");
                }
            }
            else if (StudentId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a student first.";
                return RedirectToPage("/InstructorDashboard/Index");
            }
            // =========================================================

            try
            {
                // 1. Load Student Profile
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    StudentOperations studentOps = new StudentOperations();
                    Student searchCriteria = new Student { Id = StudentId };
                    var students = studentOps.Select(conn, searchCriteria, out string message, out int status);

                    if (students != null && students.Any())
                        StudentProfile = students.First();
                    else
                    {
                        TempData["ErrorMessage"] = "Could not find a student profile for this ID.";
                        return Page();
                    }
                }

                // 2. Load Curriculum and Find the BEST Enrollment History
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 🌟 FIX: Query now includes Student's Dept PLUS Freshman/General/Prep Departments
                    string query = @"
                        SELECT 
                            c.Code AS CourseCode,
                            c.Name AS CourseName,
                            c.Credit_Hours AS CreditHours,
                            ISNULL(e.StatusDescription, 'Not Taken') AS StatusDescription,
                            e.Midterm_Grade,
                            e.CourseWork_Grade,
                            e.Final_Grade
                        FROM (
                            SELECT DISTINCT c.Id, c.Code, c.Name, c.Credit_Hours
                            FROM [Courses] c
                            LEFT JOIN [Courses_Departments] cd ON c.Id = cd.Course_id
                            LEFT JOIN [Departments] d ON cd.Department_id = d.Id
                            WHERE cd.Department_id = (SELECT Department_id FROM [Students] WHERE Id = @StudentId)
                               OR d.Name LIKE '%Freshman%'
                               OR d.Name LIKE '%General%'
                               OR d.Name LIKE '%Prep%'
                               OR cd.Department_id IS NULL
                        ) c
                        OUTER APPLY (
                            SELECT TOP 1 
                                es.Description AS StatusDescription,
                                g.Midterm_Grade,
                                g.CourseWork_Grade,
                                g.Final_Grade
                            FROM [Enrollments] en
                            LEFT JOIN [EnrollmentsStatus] es ON en.Status_Id = es.Id
                            LEFT JOIN [Grades] g ON g.Enrollment_Id = en.Id
                            WHERE en.Course_Id = c.Id AND en.Student_Id = @StudentId
                            ORDER BY 
                                CASE 
                                    WHEN es.Description = 'Completed' THEN 1
                                    WHEN (ISNULL(g.Midterm_Grade,0) + ISNULL(g.CourseWork_Grade,0) + ISNULL(g.Final_Grade,0)) >= 60 THEN 1
                                    WHEN es.Description = 'Enrolled' OR es.Description = 'Active' THEN 2
                                    ELSE 3
                                END ASC, 
                                en.Id DESC
                        ) e;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", StudentId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int credits = reader["CreditHours"] != DBNull.Value ? Convert.ToInt32(reader["CreditHours"]) : 3;
                                string courseStatus = reader["StatusDescription"]?.ToString() ?? "Not Taken";

                                decimal? midterm = reader["Midterm_Grade"] != DBNull.Value ? Convert.ToDecimal(reader["Midterm_Grade"]) : (decimal?)null;
                                decimal? coursework = reader["CourseWork_Grade"] != DBNull.Value ? Convert.ToDecimal(reader["CourseWork_Grade"]) : (decimal?)null;
                                decimal? final = reader["Final_Grade"] != DBNull.Value ? Convert.ToDecimal(reader["Final_Grade"]) : (decimal?)null;

                                decimal? total = null;
                                if (midterm.HasValue && coursework.HasValue && final.HasValue)
                                {
                                    total = midterm.Value + coursework.Value + final.Value;
                                }

                                bool isPassed = courseStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                                                (total.HasValue && total.Value >= 60);

                                if (isPassed)
                                {
                                    TotalEarnedCredits += credits;
                                    courseStatus = "Completed";
                                }

                                DegreeCourses.Add(new DegreePlanItem
                                {
                                    CourseCode = reader["CourseCode"]?.ToString() ?? "CRS",
                                    CourseName = reader["CourseName"]?.ToString() ?? "Unknown",
                                    CreditHours = credits,
                                    Status = courseStatus,
                                    TotalGrade = total,
                                    LetterGrade = GetLetterGrade(total, courseStatus)
                                });
                            }
                        }
                    }
                }

                // Custom sorting: Completed -> Enrolled -> Failed/Dropped -> Not Taken
                DegreeCourses = DegreeCourses.OrderBy(c =>
                {
                    string status = c.Status.ToLower();
                    if (status == "completed") return 1;
                    if (status == "enrolled" || status == "active") return 2;
                    if (status == "not taken") return 4;
                    return 3;
                })
                .ThenBy(c => c.CourseCode)
                .ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Database error: " + ex.Message;
            }

            return Page();
        }

        private string GetLetterGrade(decimal? score, string status)
        {
            if (status.Equals("Not Taken", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Enrolled", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return "-";
            }

            if (!score.HasValue) return "-";
            double s = (double)score.Value;

            if (s >= 90) return "A+";
            if (s >= 85) return "A";
            if (s >= 80) return "B+";
            if (s >= 75) return "B";
            if (s >= 70) return "C+";
            if (s >= 65) return "C";
            if (s >= 60) return "D";
            return "F";
        }
    }

    public class DegreePlanItem
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public string Status { get; set; } = "Not Taken";
        public decimal? TotalGrade { get; set; }
        public string LetterGrade { get; set; } = "-";
    }
}