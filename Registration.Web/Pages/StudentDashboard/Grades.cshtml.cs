using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess;
using Registration.Domain;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Registration.Web.Pages.StudentDashboard
{
    public class GradesModel : PageModel
    {
        private readonly string _connectionString;

        public GradesModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [BindProperty(SupportsGet = true)]
        public int StudentId { get; set; }

        public StudentDetailsView StudentProfile { get; set; } = new();

        public string CurrentSemesterName { get; set; } = "Unknown Semester";
        public int TotalSemesterCredits { get; set; } = 0;
        public List<SemesterGradeItem> SemesterGrades { get; set; } = new();

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
                // 2. Load Student Profile
                using (SqlConnection profileConn = new SqlConnection(_connectionString))
                {
                    profileConn.Open();
                    StudentOperations studentOps = new StudentOperations();
                    Student searchCriteria = new Student { Id = StudentId };
                    var students = studentOps.Select(profileConn, searchCriteria, out string message, out int status);

                    if (students != null && students.Any())
                        StudentProfile = students.First();
                    else
                    {
                        TempData["ErrorMessage"] = "Could not find a student profile for this ID.";
                        return Page();
                    }
                }

                // 3. Load Grades for the Intelligently Selected Current Semester
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // 🌟 INTELLIGENT DATE-BASED SEMESTER RESOLUTION
                    string query = @"
                        DECLARE @ActiveSemesterId INT;
                        DECLARE @ActiveSemesterName NVARCHAR(100);

                        -- Priority 1: Current live semester based on real-world today's date
                        SELECT TOP 1 @ActiveSemesterId = Id, @ActiveSemesterName = Name 
                        FROM [Semesters] 
                        WHERE CAST(GETDATE() AS DATE) BETWEEN Start_date AND End_date;

                        -- Priority 2: Fallback to the most recent semester that has already started (e.g., during breaks)
                        IF @ActiveSemesterId IS NULL
                        BEGIN
                            SELECT TOP 1 @ActiveSemesterId = Id, @ActiveSemesterName = Name 
                            FROM [Semesters] 
                            WHERE Start_date <= CAST(GETDATE() AS DATE)
                            ORDER BY Start_date DESC;
                        END

                        -- Priority 3: Absolute fallback to the newest semester in the database
                        IF @ActiveSemesterId IS NULL
                        BEGIN
                            SELECT TOP 1 @ActiveSemesterId = Id, @ActiveSemesterName = Name 
                            FROM [Semesters] 
                            ORDER BY Start_date DESC;
                        END

                        SELECT ISNULL(@ActiveSemesterName, 'No Active Semester') AS SemesterName;

                        SELECT 
                            c.Name AS CourseName,
                            c.Credit_Hours AS CreditHours,
                            g.Midterm_Grade AS MidtermGrade,
                            g.CourseWork_Grade AS CourseWorkGrade,
                            g.Final_Grade AS FinalGrade,
                            es.Description AS StatusDescription
                        FROM [Enrollments] e
                        INNER JOIN [Courses] c ON e.Course_Id = c.Id
                        INNER JOIN [EnrollmentsStatus] es ON e.Status_Id = es.Id
                        LEFT JOIN [Grades] g ON g.Enrollment_Id = e.Id
                        WHERE e.Student_Id = @StudentId 
                        AND e.Semester_Id = @ActiveSemesterId;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", StudentId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentSemesterName = reader["SemesterName"]?.ToString() ?? "Unknown";
                            }

                            if (reader.NextResult())
                            {
                                TotalSemesterCredits = 0;
                                while (reader.Read())
                                {
                                    int credits = reader["CreditHours"] != DBNull.Value ? Convert.ToInt32(reader["CreditHours"]) : 3;
                                    if (credits <= 0) credits = 3;
                                    TotalSemesterCredits += credits;

                                    decimal? midterm = reader["MidtermGrade"] != DBNull.Value ? Convert.ToDecimal(reader["MidtermGrade"]) : (decimal?)null;
                                    decimal? coursework = reader["CourseWorkGrade"] != DBNull.Value ? Convert.ToDecimal(reader["CourseWorkGrade"]) : (decimal?)null;
                                    decimal? final = reader["FinalGrade"] != DBNull.Value ? Convert.ToDecimal(reader["FinalGrade"]) : (decimal?)null;

                                    decimal? total = null;
                                    if (midterm.HasValue && coursework.HasValue && final.HasValue)
                                    {
                                        total = midterm.Value + coursework.Value + final.Value;
                                    }

                                    SemesterGrades.Add(new SemesterGradeItem
                                    {
                                        CourseName = reader["CourseName"]?.ToString() ?? "Unknown",
                                        Midterm = midterm,
                                        Coursework = coursework,
                                        Final = final,
                                        Total = total,
                                        Status = reader["StatusDescription"]?.ToString() ?? "Unknown"
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Database error: " + ex.Message;
            }

            return Page();
        }

        public string CalculateSemesterGpa()
        {
            if (SemesterGrades == null || !SemesterGrades.Any()) return "0.00";

            double totalPoints = 0;
            int gradedCoursesCount = 0;

            foreach (var grade in SemesterGrades)
            {
                if (grade.Total.HasValue)
                {
                    totalPoints += ConvertScoreToGpa((double)grade.Total.Value);
                    gradedCoursesCount++;
                }
            }

            if (gradedCoursesCount == 0) return "0.00";
            return (totalPoints / gradedCoursesCount).ToString("0.00");
        }

        private double ConvertScoreToGpa(double score)
        {
            if (score >= 90) return 4.0;
            if (score >= 85) return 3.7;
            if (score >= 80) return 3.3;
            if (score >= 75) return 3.0;
            if (score >= 70) return 2.7;
            if (score >= 65) return 2.3;
            if (score >= 60) return 2.0;
            if (score >= 55) return 1.7;
            if (score >= 50) return 1.0;
            return 0.0;
        }
    }

    public class SemesterGradeItem
    {
        public string CourseName { get; set; } = string.Empty;
        public decimal? Midterm { get; set; }
        public decimal? Coursework { get; set; }
        public decimal? Final { get; set; }
        public decimal? Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}