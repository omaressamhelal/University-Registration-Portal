using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Registration.Domain;

namespace Registration.Web.Pages.InstructorDashboard
{
    public class CourseRosterModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CourseRosterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("RegistrationApi");
        }

        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; }

        // 🌟 NEW: Search query binding for the UI search bar
        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; } = string.Empty;

        public List<EnrollmentDetailsView> EnrolledStudents { get; set; } = new List<EnrollmentDetailsView>();
        public CourseDetailsView CourseDetails { get; set; } = new CourseDetailsView();
        public List<LectureDateView> PastLectures { get; set; } = new List<LectureDateView>();

        [BindProperty]
        public DateTime AttendanceDate { get; set; } = DateTime.Now;

        [BindProperty]
        public string Content_Covered { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedScheduleType { get; set; } = "Lecture";

        public int TotalStudents => EnrolledStudents.Count;

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            CourseId = courseId;
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            await LoadRosterDataAsync(courseId, jsonOptions);

            // Filter students dynamically by name if a search query is provided
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                EnrolledStudents = EnrolledStudents.Where(s =>
                    !string.IsNullOrEmpty(s.StudentName) && s.StudentName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Page();
        }

        private async Task LoadRosterDataAsync(int courseId, JsonSerializerOptions jsonOptions)
        {
            var rosterResponse = await _httpClient.GetAsync($"api/enrollments?courseId={courseId}");
            if (rosterResponse.IsSuccessStatusCode)
            {
                var rosterJson = await rosterResponse.Content.ReadAsStringAsync();
                EnrolledStudents = JsonSerializer.Deserialize<List<EnrollmentDetailsView>>(rosterJson, jsonOptions) ?? new List<EnrollmentDetailsView>();
            }

            var courseResponse = await _httpClient.GetAsync($"api/courses/{courseId}");
            if (courseResponse.IsSuccessStatusCode)
            {
                var courseJson = await courseResponse.Content.ReadAsStringAsync();
                CourseDetails = JsonSerializer.Deserialize<CourseDetailsView>(courseJson, jsonOptions) ?? new CourseDetailsView();
            }

            var lecturesResponse = await _httpClient.GetAsync($"api/LectureEvents/Course/{courseId}");
            if (lecturesResponse.IsSuccessStatusCode)
            {
                var lecturesJson = await lecturesResponse.Content.ReadAsStringAsync();
                PastLectures = JsonSerializer.Deserialize<List<LectureDateView>>(lecturesJson, jsonOptions) ?? new List<LectureDateView>();
            }
        }

        public async Task<IActionResult> OnPostCreateLectureAsync(int courseId)
        {
            if (courseId <= 0) courseId = CourseId;
            CourseId = courseId;
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var lecturesResponse = await _httpClient.GetAsync($"api/LectureEvents/Course/{courseId}");
            if (lecturesResponse.IsSuccessStatusCode)
            {
                var lecturesJson = await lecturesResponse.Content.ReadAsStringAsync();
                var existingLectures = JsonSerializer.Deserialize<List<LectureDateView>>(lecturesJson, jsonOptions);

                if (existingLectures != null && existingLectures.Any())
                {
                    var latestDate = existingLectures.Max(l => l.AttendanceDate).Date;

                    if (AttendanceDate.Date <= latestDate)
                    {
                        TempData["ErrorMessage"] = $"Validation Failed: The new session date must be after the latest recorded session ({latestDate:dd/MM/yyyy}).";
                        return RedirectToPage(new { courseId = courseId });
                    }
                }
            }

            int scheduleId = 0;
            string safeType = string.IsNullOrWhiteSpace(SelectedScheduleType) ? "Lecture" : SelectedScheduleType;

            var scheduleResponse = await _httpClient.GetAsync($"api/CourseSchedules/GetOrCreate/{courseId}?type={safeType}");

            if (!scheduleResponse.IsSuccessStatusCode)
            {
                var apiError = await scheduleResponse.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"API Error creating '{safeType}' schedule: {apiError}";
                return RedirectToPage(new { courseId = courseId });
            }

            var idStr = await scheduleResponse.Content.ReadAsStringAsync();
            if (int.TryParse(idStr, out int parsedId))
            {
                scheduleId = parsedId;
            }

            if (scheduleId == 0)
            {
                TempData["ErrorMessage"] = "API succeeded but returned an invalid Schedule ID.";
                return RedirectToPage(new { courseId = courseId });
            }

            var newLecture = new
            {
                ScheduleId = scheduleId,
                DateOfEvent = AttendanceDate,
                ContentCovered = Content_Covered
            };

            var content = new StringContent(JsonSerializer.Serialize(newLecture), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/LectureEvents", content);

            if (response.IsSuccessStatusCode)
            {
                var dateString = AttendanceDate.ToString("yyyy-MM-dd");

                var idResponse = await _httpClient.GetAsync($"api/LectureEvents/GetId?scheduleId={scheduleId}&date={dateString}");

                if (idResponse.IsSuccessStatusCode)
                {
                    var newEventIdStr = await idResponse.Content.ReadAsStringAsync();
                    if (int.TryParse(newEventIdStr, out int newLectureEventId))
                    {
                        return RedirectToPage("./Attendance", new { courseId = courseId, scheduleId = newLectureEventId });
                    }
                }

                TempData["ErrorMessage"] = "Session created, but couldn't retrieve the new ID to redirect.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to create session. Server responded: {errorMsg}";
            }

            return RedirectToPage(new { courseId = courseId });
        }
    }
}