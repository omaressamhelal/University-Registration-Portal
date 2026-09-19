using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess;
using Registration.Domain;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Registration.Web.Pages.StudentDashboard
{
    public class TranscriptModel : PageModel
    {
        private readonly string _connectionString;

        public TranscriptModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [BindProperty(SupportsGet = true)]
        public int StudentId { get; set; }

        public StudentDetailsView StudentProfile { get; set; } = new();
        public int TotalCoursesTaken { get; set; } = 0;
        public List<SemesterTranscriptGroup> Semesters { get; set; } = new();

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
                // 2. Load student profile details
                using (var profileConn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
                {
                    profileConn.Open();
                    var studentOps = new StudentOperations();
                    var searchCriteria = new Student { Id = StudentId };
                    var students = studentOps.Select(profileConn, searchCriteria, out string message, out int status);

                    if (students != null && students.Any())
                        StudentProfile = students.First();
                    else
                    {
                        TempData["ErrorMessage"] = "Could not find a student profile for this ID.";
                        return Page();
                    }
                }

                // 3. Load transcript history grouped by semesters
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            sem.Id AS SemesterId,
                            sem.Name AS SemesterName,
                            c.Id AS CourseId,
                            c.Code AS CourseCode,
                            c.Name AS CourseName,
                            c.Credit_Hours AS CreditHours,
                            g.Midterm_Grade AS Midterm,
                            g.CourseWork_Grade AS Coursework,
                            g.Final_Grade AS Final,
                            es.Description AS StatusDescription
                        FROM [Enrollments] e
                        INNER JOIN [Semesters] sem ON e.Semester_Id = sem.Id
                        INNER JOIN [Courses] c ON e.Course_Id = c.Id
                        INNER JOIN [EnrollmentsStatus] es ON e.Status_Id = es.Id
                        LEFT JOIN [Grades] g ON g.Enrollment_Id = e.Id
                        WHERE e.Student_Id = @StudentId
                        ORDER BY sem.Start_date DESC, c.Name ASC;";

                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", StudentId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            var semesterDict = new Dictionary<int, SemesterTranscriptGroup>();

                            while (reader.Read())
                            {
                                int semId = Convert.ToInt32(reader["SemesterId"]);
                                string semName = reader["SemesterName"]?.ToString() ?? "Unknown Semester";

                                if (!semesterDict.ContainsKey(semId))
                                {
                                    semesterDict[semId] = new SemesterTranscriptGroup
                                    {
                                        SemesterId = semId,
                                        SemesterName = semName,
                                        Courses = new List<TranscriptCourseItem>()
                                    };
                                }

                                int credits = reader["CreditHours"] != DBNull.Value ? Convert.ToInt32(reader["CreditHours"]) : 3;
                                if (credits <= 0) credits = 3;

                                decimal? midterm = reader["Midterm"] != DBNull.Value ? Convert.ToDecimal(reader["Midterm"]) : (decimal?)null;
                                decimal? coursework = reader["Coursework"] != DBNull.Value ? Convert.ToDecimal(reader["Coursework"]) : (decimal?)null;
                                decimal? final = reader["Final"] != DBNull.Value ? Convert.ToDecimal(reader["Final"]) : (decimal?)null;

                                decimal? total = null;
                                if (midterm.HasValue && coursework.HasValue && final.HasValue)
                                {
                                    total = midterm.Value + coursework.Value + final.Value;
                                }

                                semesterDict[semId].Courses.Add(new TranscriptCourseItem
                                {
                                    CourseCode = reader["CourseCode"]?.ToString() ?? "CRS",
                                    CourseName = reader["CourseName"]?.ToString() ?? "Unknown Course",
                                    CreditHours = credits,
                                    TotalGrade = total,
                                    Status = reader["StatusDescription"]?.ToString() ?? "Enrolled"
                                });

                                TotalCoursesTaken++;
                            }

                            Semesters = semesterDict.Values.ToList();
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

        public string CalculateCumulativeGpa()
        {
            if (Semesters == null || !Semesters.Any()) return "0.00";

            double totalPoints = 0;
            int totalCredits = 0;

            foreach (var sem in Semesters)
            {
                foreach (var course in sem.Courses)
                {
                    if (course.TotalGrade.HasValue)
                    {
                        double gpaPoints = ConvertScoreToGpa((double)course.TotalGrade.Value);
                        totalPoints += gpaPoints * course.CreditHours;
                        totalCredits += course.CreditHours;
                    }
                }
            }

            if (totalCredits == 0) return "0.00";
            return (totalPoints / totalCredits).ToString("0.00");
        }

        public string GetLetterGrade(decimal? score)
        {
            if (!score.HasValue) return "N/A";
            double s = (double)score.Value;

            if (s >= 90) return "A+ (4.0)";
            if (s >= 85) return "A (3.7)";
            if (s >= 80) return "B+ (3.3)";
            if (s >= 75) return "B (3.0)";
            if (s >= 70) return "C+ (2.7)";
            if (s >= 65) return "C (2.3)";
            if (s >= 60) return "D (2.0)";
            return "F (0.0)";
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

    public class SemesterTranscriptGroup
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public List<TranscriptCourseItem> Courses { get; set; } = new();
        public int TotalCredits => Courses.Sum(c => c.CreditHours);
    }

    public class TranscriptCourseItem
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public decimal? TotalGrade { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}