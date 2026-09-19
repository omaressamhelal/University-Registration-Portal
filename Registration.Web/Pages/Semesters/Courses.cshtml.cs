using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.Semesters
{
    public class CoursesModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CoursesModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty(SupportsGet = true)]
        public int SemesterId { get; set; }

        public string SemesterName { get; set; } = "Semester";

        public List<SemesterCourseView> SemesterCoursesList { get; set; } = new();

        // For adding a course
        [BindProperty]
        public SemesterCourseView NewAssignment { get; set; } = new();

        // Dropdown list for available courses to add in the modal
        public List<Course> AllCoursesList { get; set; } = new();

        // NEW: Dropdown list for the department filter toolbar
        public List<Department> AllDepartmentsList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (SemesterId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid semester selected.";
                return RedirectToPage("/Semesters/Index");
            }

            await LoadDataAsync();
            return Page();
        }

        // ==========================================
        // HANDLER: Add Course to Semester
        // ==========================================
        public async Task<IActionResult> OnPostAddCourseAsync()
        {
            NewAssignment.Semester_Id = SemesterId;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/semestercourses", NewAssignment);

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Course successfully added to the semester!";
                else
                    TempData["ErrorMessage"] = "API Error: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToPage(new { semesterId = SemesterId });
        }

        // ==========================================
        // HANDLER: Remove Course from Semester
        // ==========================================
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"https://127.0.0.1:7126/api/semestercourses/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Course removed from semester successfully!";
                else
                    TempData["ErrorMessage"] = "Failed to remove course: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToPage(new { semesterId = SemesterId });
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // 1. Get courses assigned to this semester
                var response = await _httpClient.GetFromJsonAsync<List<SemesterCourseView>>($"https://127.0.0.1:7126/api/semestercourses?semesterId={SemesterId}");
                if (response != null)
                {
                    SemesterCoursesList = response;
                    if (SemesterCoursesList.Count > 0 && !string.IsNullOrEmpty(SemesterCoursesList[0].SemesterName))
                    {
                        SemesterName = SemesterCoursesList[0].SemesterName;
                    }
                }

                // 2. Also fetch the semester name directly if the list is empty
                if (SemesterName == "Semester")
                {
                    var semResponse = await _httpClient.GetFromJsonAsync<Semester>($"https://127.0.0.1:7126/api/semesters/{SemesterId}");
                    if (semResponse != null) SemesterName = semResponse.Name;
                }

                // 3. Get all general courses to populate the "Add Course" dropdown modal
                var coursesResponse = await _httpClient.GetFromJsonAsync<List<Course>>("https://127.0.0.1:7126/api/courses");
                if (coursesResponse != null) AllCoursesList = coursesResponse;

                // 4. NEW: Get all departments to populate the filter dropdown toolbar
                var deptResponse = await _httpClient.GetFromJsonAsync<List<Department>>("https://127.0.0.1:7126/api/departments");
                if (deptResponse != null) AllDepartmentsList = deptResponse;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not load data from API: " + ex.Message;
            }
        }
    }
}