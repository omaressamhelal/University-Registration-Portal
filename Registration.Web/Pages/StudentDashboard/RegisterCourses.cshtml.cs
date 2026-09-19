using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using Registration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.StudentDashboard
{
    [Authorize(Roles = "Student")]
    public class RegisterCoursesModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty(SupportsGet = true)]
        public int StudentId { get; set; }

        public StudentDetailsView StudentProfile { get; set; } = new();
        public string CurrentSemesterName { get; set; } = string.Empty;
        public int CurrentSemesterId { get; set; }
        public bool IsRegistrationOpen { get; set; } = true;

        public List<CourseDetailsView> AvailableCourses { get; set; } = new();
        public HashSet<int> EnrolledCourseIds { get; set; } = new();

        public RegisterCoursesModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (StudentId <= 0) return RedirectToPage("./Index");

            try
            {
                // 1. Load Student Profile
                var studentRes = await _httpClient.GetAsync("https://127.0.0.1:7126/api/students");
                if (studentRes.IsSuccessStatusCode)
                {
                    var list = JsonSerializer.Deserialize<List<StudentDetailsView>>(await studentRes.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    StudentProfile = list?.FirstOrDefault(s => s.Id == StudentId) ?? new();
                }

                // 2. 🌟 REGISTRATION-BASED LOGIC: Find the semester open for registration
                var semRes = await _httpClient.GetAsync("https://127.0.0.1:7126/api/semesters");
                if (semRes.IsSuccessStatusCode)
                {
                    var semesters = JsonSerializer.Deserialize<List<Semester>>(await semRes.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (semesters != null && semesters.Any())
                    {
                        // Priority 1: Hunt for the semester specifically marked for Open Registration
                        var targetSemester = semesters.Where(s => s.Is_Registration_Open ?? false).OrderByDescending(s => s.Start_date).FirstOrDefault();

                        if (targetSemester != null)
                        {
                            CurrentSemesterId = targetSemester.Id;
                            CurrentSemesterName = targetSemester.Name;
                            IsRegistrationOpen = true;
                        }
                        else
                        {
                            // Priority 2: If no registration is open, fallback to the real-time active semester, but LOCK registration.
                            DateTime today = DateTime.Today;
                            targetSemester = semesters.FirstOrDefault(s => s.Start_date <= today && s.End_date >= today)
                                          ?? semesters.Where(s => s.Start_date <= today).OrderByDescending(s => s.Start_date).FirstOrDefault()
                                          ?? semesters.OrderByDescending(s => s.Start_date).FirstOrDefault();

                            if (targetSemester != null)
                            {
                                CurrentSemesterId = targetSemester.Id;
                                CurrentSemesterName = targetSemester.Name;
                            }
                            IsRegistrationOpen = false; // Block UI because no semester is open
                        }
                    }
                }

                // 3. Load all courses, tag Year 1 courses as "Freshman", and filter
                var coursesRes = await _httpClient.GetAsync("https://127.0.0.1:7126/api/courses");
                if (coursesRes.IsSuccessStatusCode)
                {
                    var allCourses = JsonSerializer.Deserialize<List<CourseDetailsView>>(await coursesRes.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                    foreach (var c in allCourses)
                    {
                        if (!string.IsNullOrEmpty(c.Code) && (
                            c.Code.StartsWith("1") ||
                            c.Code.Contains("101") ||
                            c.Code.Contains("102") ||
                            System.Text.RegularExpressions.Regex.IsMatch(c.Code, "^[A-Za-z]1")
                        ))
                        {
                            c.DepartmentName = "Freshman";
                        }
                    }

                    // Filter: Show only Student's Department OR "Freshman" courses
                    AvailableCourses = allCourses.Where(c =>
                        string.Equals(c.DepartmentName?.Trim(), StudentProfile.DepartmentName?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.DepartmentName?.Trim(), "Freshman", StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // 4. Load existing enrollments for the targeted semester
                var enrollRes = await _httpClient.GetAsync("https://127.0.0.1:7126/api/enrollments");
                if (enrollRes.IsSuccessStatusCode)
                {
                    var allEnrollments = JsonSerializer.Deserialize<List<EnrollmentDetailsView>>(await enrollRes.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                    EnrolledCourseIds = allEnrollments
                        .Where(e => e.StudentId == StudentId && e.SemesterId == CurrentSemesterId)
                        .Select(e => e.CourseId)
                        .ToHashSet();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading portal: " + ex.Message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEnrollAsync(int courseId)
        {
            if (StudentId <= 0 || courseId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid enrollment request.";
                return RedirectToPage(new { studentId = StudentId });
            }

            try
            {
                var semRes = await _httpClient.GetAsync("https://127.0.0.1:7126/api/semesters");
                int semesterId = 0;
                bool isRegistrationOpen = false;

                if (semRes.IsSuccessStatusCode)
                {
                    var semesters = JsonSerializer.Deserialize<List<Semester>>(await semRes.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // 🌟 SECURE POST CHECK: Re-verify that there is an openly active registration semester
                    var targetSemester = semesters?.Where(s => s.Is_Registration_Open ?? false).OrderByDescending(s => s.Start_date).FirstOrDefault();

                    if (targetSemester != null)
                    {
                        semesterId = targetSemester.Id;
                        isRegistrationOpen = true;
                    }
                }

                // 🛡️ Block enrollment strictly if registration is closed
                if (!isRegistrationOpen)
                {
                    TempData["ErrorMessage"] = "Course registration is currently closed. 🔒";
                    return RedirectToPage(new { studentId = StudentId });
                }

                var newEnrollmentPayload = new
                {
                    StudentId = StudentId,
                    CourseId = courseId,
                    StatusId = EnrollmentStatus.Pending,
                    SemesterId = semesterId,
                    Semester_Id = semesterId
                };

                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/enrollments", newEnrollmentPayload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Course registered successfully as Pending! ⏳";
                }
                else
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorDetails)
                        ? "Failed to register for course."
                        : errorDetails.Trim('"');
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Registration error: " + ex.Message;
            }

            return RedirectToPage(new { studentId = StudentId });
        }
    }
}