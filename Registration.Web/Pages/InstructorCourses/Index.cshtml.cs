using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System.Net.Http.Json;
using System.Linq;

namespace Registration.Web.Pages.InstructorCourses
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://127.0.0.1:7126/api/instructorcourses";

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<InstructorCourseDetailsView> AssignmentsList { get; set; } = new();
        public List<InstructorDetailsView> InstructorsList { get; set; } = new();
        public List<Course> CoursesList { get; set; } = new();
        public List<Semester> SemestersList { get; set; } = new();

        // 🌟 Filter property bound from the dropdown query string/form
        [BindProperty(SupportsGet = true)]
        public int? SelectedSemesterId { get; set; }

        [BindProperty]
        public InstructorCourse NewAssignment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownDataAsync();

            // If no semester is explicitly chosen via dropdown, default to the newest one chronologically
            if (!SelectedSemesterId.HasValue && SemestersList.Any())
            {
                var newestSemester = SemestersList
                    .OrderByDescending(s => s.Start_date)
                    .FirstOrDefault();

                if (newestSemester != null)
                {
                    SelectedSemesterId = newestSemester.Id;
                }
            }

            await LoadAssignmentsAsync(SelectedSemesterId);
            return Page();
        }

        public async Task<IActionResult> OnPostAssignCourseAsync()
        {
            try
            {
                // 1. Ensure SemestersList is loaded
                await LoadDropdownDataAsync();

                // 2. 🌟 Forcefully lock the semester to the globally newest/last semester
                var newestSemester = SemestersList.OrderByDescending(s => s.Start_date).FirstOrDefault();
                if (newestSemester != null)
                {
                    NewAssignment.Semester_Id = newestSemester.Id;
                }

                var response = await _httpClient.PostAsJsonAsync(_apiUrl, NewAssignment);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Instructor successfully assigned to the course for the latest semester!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(error) ? "Failed to assign course." : error;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Connection Error: {ex.Message}";
            }

            return RedirectToPage(new { SelectedSemesterId });
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Assignment removed successfully!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(error) ? "Failed to remove assignment." : error;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Connection Error: {ex.Message}";
            }

            return RedirectToPage(new { SelectedSemesterId });
        }

        private async Task LoadAssignmentsAsync(int? semesterId)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<InstructorCourseDetailsView>>(_apiUrl);
                if (data != null)
                {
                    // Filter assignments by the selected semester if specified
                    if (semesterId.HasValue && semesterId.Value > 0)
                    {
                        AssignmentsList = data.Where(a => a.Semester_Id == semesterId.Value).ToList();
                    }
                    else
                    {
                        AssignmentsList = data;
                    }
                }
            }
            catch { AssignmentsList = new(); }
        }

        private async Task LoadDropdownDataAsync()
        {
            try
            {
                var instructors = await _httpClient.GetFromJsonAsync<List<InstructorDetailsView>>("https://127.0.0.1:7126/api/instructors");
                if (instructors != null) InstructorsList = instructors;

                var courses = await _httpClient.GetFromJsonAsync<List<Course>>("https://127.0.0.1:7126/api/courses");
                if (courses != null) CoursesList = courses;

                var semesters = await _httpClient.GetFromJsonAsync<List<Semester>>("https://127.0.0.1:7126/api/semesters");
                if (semesters != null)
                {
                    // Order semesters newest to oldest for the dropdown list
                    SemestersList = semesters.OrderByDescending(s => s.Start_date).ToList();
                }
            }
            catch { /* Keep lists empty on failure */ }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            return await OnPostRemoveAsync(id);
        }
    }
}