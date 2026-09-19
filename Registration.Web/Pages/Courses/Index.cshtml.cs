using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.Courses
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<CourseDetailsView> CoursesList { get; set; } = new();
        public List<string> StatusesList { get; set; } = new();
        public List<DepartmentDetailsView> DepartmentsList { get; set; } = new();

        [BindProperty]
        public Course NewCourse { get; set; } = new();

        [BindProperty]
        public int SelectedDepartmentId { get; set; } // Captures the dropdown selection from modals

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                CoursesList = await _httpClient.GetFromJsonAsync<List<CourseDetailsView>>("https://127.0.0.1:7126/api/courses") ?? new();
                DepartmentsList = await _httpClient.GetFromJsonAsync<List<DepartmentDetailsView>>("https://127.0.0.1:7126/api/departments") ?? new();

                var statuses = await _httpClient.GetFromJsonAsync<List<string>>("https://127.0.0.1:7126/api/courses/statuses");
                if (statuses != null)
                {
                    StatusesList = statuses;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "API Connection Error: " + ex.Message;
                CoursesList = new();
                StatusesList = new();
                DepartmentsList = new();
            }
        }

        public async Task<IActionResult> OnPostCreateCourseAsync()
        {
            var payload = new
            {
                Course = NewCourse,
                DepartmentId = SelectedDepartmentId
            };

            var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/courses", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "New course added successfully.";
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to add course: {errorMessage}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateCourseAsync()
        {
            var payload = new
            {
                Course = NewCourse,
                DepartmentId = SelectedDepartmentId
            };

            var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/courses/{NewCourse.Id}", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course details updated successfully.";
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to update course: {errorMessage}";
            }

            return RedirectToPage();
        }
    }
}