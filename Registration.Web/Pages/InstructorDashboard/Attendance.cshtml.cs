using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Registration.Web.Pages.InstructorDashboard
{
    // 🌟 SECURED: Only instructors can access and save attendance
    [Authorize(Roles = "Instructor")]
    public class AttendanceModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public AttendanceModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("RegistrationApi");
        }

        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ScheduleId { get; set; }

        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string ScheduleType { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }

        public List<AttendanceDetailsView> AttendanceList { get; set; } = new List<AttendanceDetailsView>();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int courseId, int scheduleId)
        {
            CourseId = courseId;
            ScheduleId = scheduleId;
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var attendanceResponse = await _httpClient.GetAsync($"api/Attendance?scheduleId={scheduleId}&courseId={courseId}");
            if (attendanceResponse.IsSuccessStatusCode)
            {
                var attendanceJson = await attendanceResponse.Content.ReadAsStringAsync();
                AttendanceList = JsonSerializer.Deserialize<List<AttendanceDetailsView>>(attendanceJson, jsonOptions) ?? new List<AttendanceDetailsView>();

                if (AttendanceList.Any())
                {
                    var firstRecord = AttendanceList.First();
                    CourseName = firstRecord.CourseName;
                    CourseCode = firstRecord.CourseCode;
                    ScheduleType = firstRecord.ScheduleType;
                    AttendanceDate = firstRecord.AttendanceDate;
                }
                else
                {
                    var courseResponse = await _httpClient.GetAsync($"api/courses/{courseId}");
                    if (courseResponse.IsSuccessStatusCode)
                    {
                        var courseJson = await courseResponse.Content.ReadAsStringAsync();
                        var course = JsonSerializer.Deserialize<CourseDetailsView>(courseJson, jsonOptions);
                        if (course != null)
                        {
                            CourseName = course.Name;
                            CourseCode = course.Code;
                        }
                    }
                }
            }

            return Page();
        }

        // Handles saving by directly extracting checked IDs from the HTTP Request Form
        public async Task<IActionResult> OnPostSaveAttendanceAsync(int courseId, int scheduleId)
        {
            var presentStudentIds = new List<int>();

            if (Request.Form.TryGetValue("PresentStudentIds", out var selectedValues))
            {
                foreach (var val in selectedValues)
                {
                    if (int.TryParse(val, out int id))
                    {
                        presentStudentIds.Add(id);
                    }
                }
            }

            var payload = new Registration.Domain.Attendance
            {
                LectureEvents_Id = scheduleId,
                PresentStudentIds = presentStudentIds
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var content = new StringContent(JsonSerializer.Serialize(payload, jsonOptions), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Attendance/bulk-save", content);

            if (response.IsSuccessStatusCode)
            {
                StatusMessage = "Attendance saved successfully!";
            }
            else
            {
                // Capture the exact database/API error message to see what went wrong
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(errorContent);
                    if (doc.RootElement.TryGetProperty("message", out var msgProp))
                    {
                        StatusMessage = $"Error: {msgProp.GetString()}";
                    }
                    else
                    {
                        StatusMessage = $"Error: {errorContent}";
                    }
                }
                catch
                {
                    StatusMessage = "Error: Failed to save attendance changes.";
                }
            }

            return RedirectToPage(new { courseId = courseId, scheduleId = scheduleId });
        }
    }
}