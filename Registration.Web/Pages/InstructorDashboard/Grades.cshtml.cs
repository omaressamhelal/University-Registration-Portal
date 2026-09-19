using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Registration.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Registration.Web.Pages.InstructorDashboard
{
    public class GradesModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public GradesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("RegistrationApi");
        }

        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; }

        public List<GradeDetailsView> StudentGrades { get; set; } = new List<GradeDetailsView>();
        public CourseDetailsView CourseDetails { get; set; } = new CourseDetailsView();

        // 🌟 DYNAMIC API-DRIVEN COURSE NAME RESOLUTION
        public string DisplayCourseName
        {
            get
            {
                // 1. Prioritize the name returned by the api/courses/{id} endpoint
                if (!string.IsNullOrWhiteSpace(CourseDetails?.Name))
                    return CourseDetails.Name;

                // 2. Fallback to student grades view model if available
                var firstWithCourse = StudentGrades?.FirstOrDefault(g => !string.IsNullOrWhiteSpace(g.CourseName) && g.CourseName != "Course");
                if (firstWithCourse != null)
                    return firstWithCourse.CourseName;

                // 3. Ultimate fallback if API is unreachable
                return $"Course #{CourseId}";
            }
        }

        [BindProperty]
        public Dictionary<int, decimal?> Midterm { get; set; } = new Dictionary<int, decimal?>();

        [BindProperty]
        public Dictionary<int, decimal?> CourseWork { get; set; } = new Dictionary<int, decimal?>();

        [BindProperty]
        public Dictionary<int, decimal?> Final { get; set; } = new Dictionary<int, decimal?>();

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            CourseId = courseId;
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1. Fetch course details from the API endpoint we just created
            var courseResponse = await _httpClient.GetAsync($"api/courses/{courseId}");
            if (courseResponse.IsSuccessStatusCode)
            {
                var courseJson = await courseResponse.Content.ReadAsStringAsync();
                CourseDetails = JsonSerializer.Deserialize<CourseDetailsView>(courseJson, jsonOptions) ?? new CourseDetailsView();
            }

            // 2. Fetch student grades from API
            var gradesResponse = await _httpClient.GetAsync($"api/Grades/Course/{courseId}");
            if (gradesResponse.IsSuccessStatusCode)
            {
                var gradesJson = await gradesResponse.Content.ReadAsStringAsync();
                StudentGrades = JsonSerializer.Deserialize<List<GradeDetailsView>>(gradesJson, jsonOptions) ?? new List<GradeDetailsView>();

                // Ensure null total grades are explicitly normalized so the UI doesn't miscalculate them as 0
                foreach (var grade in StudentGrades)
                {
                    if (grade.MidtermGrade == null && grade.CourseWorkGrade == null && grade.FinalGrade == null)
                    {
                        grade.TotalGrade = null;
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveGradesAsync(int courseId)
        {
            CourseId = courseId;

            foreach (var key in Midterm.Keys)
            {
                int enrollmentId = key;
                decimal? mid = Midterm.ContainsKey(enrollmentId) ? Midterm[enrollmentId] : null;
                decimal? cw = CourseWork.ContainsKey(enrollmentId) ? CourseWork[enrollmentId] : null;
                decimal? fin = Final.ContainsKey(enrollmentId) ? Final[enrollmentId] : null;

                var payload = new
                {
                    Enrollment_Id = enrollmentId,
                    Midterm_Grade = mid,
                    CourseWork_Grade = cw,
                    Final_Grade = fin
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                await _httpClient.PostAsync("api/Grades", content);
            }

            TempData["SuccessMessage"] = "All grades saved successfully!";
            return RedirectToPage(new { courseId = courseId });
        }

        // 🌟 RESET HANDLER: Wipes grades and normalizes back to null state
        public async Task<IActionResult> OnPostResetGradesAsync(int courseId)
        {
            CourseId = courseId;
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var gradesResponse = await _httpClient.GetAsync($"api/Grades/Course/{courseId}");
            if (gradesResponse.IsSuccessStatusCode)
            {
                var gradesJson = await gradesResponse.Content.ReadAsStringAsync();
                var studentGrades = JsonSerializer.Deserialize<List<GradeDetailsView>>(gradesJson, jsonOptions) ?? new List<GradeDetailsView>();

                foreach (var grade in studentGrades)
                {
                    var payload = new
                    {
                        Enrollment_Id = grade.EnrollmentId,
                        Midterm_Grade = (decimal?)null,
                        CourseWork_Grade = (decimal?)null,
                        Final_Grade = (decimal?)null
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("api/Grades", content);
                }
            }

            TempData["SuccessMessage"] = "All grades have been reset successfully!";
            return RedirectToPage(new { courseId = courseId });
        }
    }
}