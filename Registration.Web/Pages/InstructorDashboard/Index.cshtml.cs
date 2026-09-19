using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.InstructorDashboard
{
    [Authorize(Roles = "Instructor")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public InstructorDetailsView InstructorProfile { get; set; }

        public List<InstructorDashboardCourseModel> ActiveCourses { get; set; } = new();

        public IndexModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(email))
            {
                var response = await _httpClient.GetAsync($"https://127.0.0.1:7126/api/instructors?email={email}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var instructorsList = JsonSerializer.Deserialize<List<InstructorDetailsView>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (instructorsList != null && instructorsList.Count > 0)
                    {
                        InstructorProfile = instructorsList[0];
                        LoadActiveCoursesForLatestSemester();
                    }
                }
            }
        }

        private void LoadActiveCoursesForLatestSemester()
        {
            if (InstructorProfile == null) return;

            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // 1. Fetch all semesters to apply intelligent date-based logic
                var allSemesters = new List<Semester>();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, Name, Start_date, End_date FROM Semesters", conn))
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
                                End_date = reader.GetDateTime(3)
                            });
                        }
                    }
                }

                if (!allSemesters.Any()) return;

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

                InstructorProfile.ActiveSemesterName = targetSemester.Name;
                int targetSemesterId = targetSemester.Id;

                // 2. Get instructor's assigned courses based on the intelligently selected semester
                string query = @"
            SELECT 
                c.Id AS CourseId,
                c.Code AS CourseCode,
                c.Name AS CourseName,
                ISNULL(cs.Day, 'TBA') AS ScheduleDay,
                CASE 
                    WHEN cs.StartTime IS NOT NULL AND cs.EndTime IS NOT NULL 
                    THEN LEFT(CAST(cs.StartTime AS VARCHAR(10)), 5) + ' - ' + LEFT(CAST(cs.EndTime AS VARCHAR(10)), 5)
                    ELSE 'TBA'
                END AS ScheduleTime,
                ISNULL(cs.Type, 'Lecture') AS ScheduleType
            FROM Instructors_Courses ic
            INNER JOIN Courses c ON ic.Course_Id = c.Id
            LEFT JOIN Course_Schedules cs ON cs.Instructors_Courses_Id = ic.Id
            WHERE ic.Instructor_Id = @InstructorId AND ic.Semester_Id = @SemesterId
        ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorId", InstructorProfile.Id);
                    cmd.Parameters.AddWithValue("@SemesterId", targetSemesterId);

                    var tempCourses = new List<InstructorDashboardCourseModel>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tempCourses.Add(new InstructorDashboardCourseModel
                            {
                                CourseId = reader.GetInt32(reader.GetOrdinal("CourseId")),
                                CourseCode = reader.GetString(reader.GetOrdinal("CourseCode")),
                                CourseName = reader.GetString(reader.GetOrdinal("CourseName")),
                                ScheduleDay = reader.GetString(reader.GetOrdinal("ScheduleDay")),
                                ScheduleTime = reader.GetString(reader.GetOrdinal("ScheduleTime")),
                                ScheduleType = reader.GetString(reader.GetOrdinal("ScheduleType"))
                            });
                        }
                    }

                    // STRONG DEDUPLICATION: Groups by Course Code so duplicate schedule rows merge into one card
                    ActiveCourses = tempCourses
                        .GroupBy(c => c.CourseCode)
                        .Select(g => g.First())
                        .ToList();
                }
            }
        }

        public IActionResult OnPostUpdateProfile(int InstructorId, string UpdatedEmail, string UpdatedOfficeHours)
        {
            if (InstructorId <= 0)
            {
                TempData["ErrorMessage"] = "Validation Failed: Invalid Instructor ID.";
                return RedirectToPage();
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var ops = new InstructorOperations();
                bool isSuccess = ops.UpdateContactInfo(conn, InstructorId, UpdatedEmail, UpdatedOfficeHours, out string msg);

                if (isSuccess)
                {
                    TempData["SuccessMessage"] = "Profile details updated successfully! 🚀";
                }
                else
                {
                    TempData["ErrorMessage"] = msg;
                }
            }

            return RedirectToPage();
        }
    }

    public class InstructorDashboardCourseModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ScheduleDay { get; set; } = "TBA";
        public string ScheduleTime { get; set; } = "TBA";
        public string ScheduleType { get; set; } = "Lecture";
    }
}