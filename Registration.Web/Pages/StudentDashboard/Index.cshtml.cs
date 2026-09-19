using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.StudentDashboard
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;

        public StudentDetailsView StudentProfile { get; set; }

        public List<StudentTranscriptView> RecentSemesterCourses { get; set; } = new();
        public string CurrentSemesterName { get; set; } = "No Enrollments";
        public bool IsRegistrationOpen { get; set; } = true; // Tracks if ANY registration is open

        public string TrueCumulativeGpa { get; set; } = "0.00";
        public int CurrentSemesterCredits { get; set; } = 0;
        public int TotalCreditsEarned { get; set; } = 0;

        public int GraduationRequiredCredits { get; set; } = 155;
        public double GraduationProgressPercentage => Math.Min(100, Math.Round((double)TotalCreditsEarned / GraduationRequiredCredits * 100, 1));

        public IndexModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task OnGetAsync()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(email))
            {
                var response = await _httpClient.GetAsync($"https://127.0.0.1:7126/api/students?email={email}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var studentsList = JsonSerializer.Deserialize<List<StudentDetailsView>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (studentsList != null && studentsList.Count > 0)
                    {
                        StudentProfile = studentsList[0];

                        LoadRecentSemesterCourses(StudentProfile.Id);
                        TrueCumulativeGpa = CalculateTrueCumulativeGpaAndCredits(StudentProfile.Id);
                    }
                }
            }
        }

        private void LoadRecentSemesterCourses(int studentId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // 1. Fetch all semesters to apply intelligent date-based logic
                    var allSemesters = new List<Semester>();
                    using (SqlCommand cmd = new SqlCommand("SELECT Id, Name, Start_date, End_date, Is_Registration_Open FROM Semesters", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allSemesters.Add(new Semester
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Start_date = reader.GetDateTime(2),
                                    End_date = reader.GetDateTime(3),
                                    Is_Registration_Open = reader.GetBoolean(4)
                                });
                            }
                        }
                    }

                    if (!allSemesters.Any()) return;

                    // 🌟 Set global registration flag for dashboard alerts (if ANY term is open)
                    IsRegistrationOpen = allSemesters.Any(s => s.Is_Registration_Open ?? false);

                    DateTime today = DateTime.Today;

                    // Priority 1: The semester we are currently living in right now
                    var targetSemester = allSemesters
                        .FirstOrDefault(s => s.Start_date <= today && s.End_date >= today);

                    // Priority 2: Fallback to the semester that most recently started (e.g., during winter break)
                    if (targetSemester == null)
                    {
                        targetSemester = allSemesters
                            .Where(s => s.Start_date <= today)
                            .OrderByDescending(s => s.Start_date)
                            .FirstOrDefault();
                    }

                    // Priority 3: Absolute fallback to the newest semester in the database
                    if (targetSemester == null)
                    {
                        targetSemester = allSemesters.OrderByDescending(s => s.Start_date).FirstOrDefault();
                    }

                    CurrentSemesterName = targetSemester.Name;
                    int targetSemesterId = targetSemester.Id;

                    // 2. Fetch specific enrollments for the intelligently selected semester
                    string query = @"
                        SELECT 
                            c.Name AS CourseName,
                            c.Code AS CourseCode,
                            c.Credit_Hours,
                            es.Description AS StatusDescription
                        FROM [Enrollments] e
                        INNER JOIN [Courses] c ON e.Course_Id = c.Id
                        INNER JOIN [EnrollmentsStatus] es ON e.Status_Id = es.Id
                        WHERE e.Student_Id = @StudentId 
                        AND e.Semester_Id = @ActiveSemesterId;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                        cmd.Parameters.AddWithValue("@ActiveSemesterId", targetSemesterId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            CurrentSemesterCredits = 0;
                            while (reader.Read())
                            {
                                int credits = reader["Credit_Hours"] != DBNull.Value ? Convert.ToInt32(reader["Credit_Hours"]) : 3;
                                if (credits <= 0) credits = 3;

                                CurrentSemesterCredits += credits;

                                RecentSemesterCourses.Add(new StudentTranscriptView
                                {
                                    CourseName = reader["CourseName"]?.ToString() ?? "",
                                    CourseCode = reader["CourseCode"]?.ToString() ?? "",
                                    StatusDescription = reader["StatusDescription"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fails silently on dashboard
            }
        }

        private string CalculateTrueCumulativeGpaAndCredits(int studentId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            c.Credit_Hours,
                            g.Midterm_Grade,
                            g.CourseWork_Grade,
                            g.Final_Grade
                        FROM [Enrollments] e
                        INNER JOIN [Courses] c ON e.Course_Id = c.Id
                        LEFT JOIN [Grades] g ON g.Enrollment_Id = e.Id
                        WHERE e.Student_Id = @StudentId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            double totalQualityPoints = 0;
                            int totalCreditHours = 0;
                            int earnedCredits = 0;

                            while (reader.Read())
                            {
                                bool hasMidterm = reader["Midterm_Grade"] != DBNull.Value;
                                bool hasCoursework = reader["CourseWork_Grade"] != DBNull.Value;
                                bool hasFinal = reader["Final_Grade"] != DBNull.Value;

                                // 🛡️ STRICT RULE: Only calculate if ALL THREE components exist
                                if (hasMidterm && hasCoursework && hasFinal)
                                {
                                    double midterm = Convert.ToDouble(reader["Midterm_Grade"]);
                                    double coursework = Convert.ToDouble(reader["CourseWork_Grade"]);
                                    double final = Convert.ToDouble(reader["Final_Grade"]);
                                    double totalGrade = midterm + coursework + final;

                                    int credits = reader["Credit_Hours"] != DBNull.Value ? Convert.ToInt32(reader["Credit_Hours"]) : 3;
                                    if (credits <= 0) credits = 3;

                                    if (totalGrade >= 50)
                                    {
                                        earnedCredits += credits;
                                    }

                                    double courseGpa = ConvertScoreToGpa(totalGrade);
                                    totalQualityPoints += (courseGpa * credits);
                                    totalCreditHours += credits;
                                }
                            }

                            TotalCreditsEarned = earnedCredits;

                            if (totalCreditHours == 0) return "0.00";
                            return (totalQualityPoints / totalCreditHours).ToString("0.00");
                        }
                    }
                }
            }
            catch
            {
                return "0.00";
            }
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
}