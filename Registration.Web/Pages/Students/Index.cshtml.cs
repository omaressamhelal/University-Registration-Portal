using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System.Net.Http.Json;

namespace Registration.Web.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<string> StatusesList { get; set; } = new();
        public List<StudentDetailsView> StudentsList { get; set; } = new();
        public List<Department> DepartmentsList { get; set; } = new();

        // Search and Pagination Properties
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        // Properties for sliding window pagination
        public int VisitedStartPage { get; set; }
        public int VisitedEndPage { get; set; }

        [BindProperty]
        public Student NewStudent { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. LOAD DEPARTMENTS FIRST so the search can translate short codes!
            await LoadDepartmentsAsync();

            await LoadStudentsAsync();
            await LoadStatusesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRegisterStudentAsync()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/students", NewStudent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Student registered successfully!";
                    return RedirectToPage();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorContent) ? "Failed to register student." : errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Connection Error: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStudentAsync()
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/students/{NewStudent.Id}", NewStudent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Student updated successfully!";
                    return RedirectToPage();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorContent) ? "Failed to update student." : errorContent;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Connection Error: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task LoadStudentsAsync()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<StudentDetailsView>>("https://127.0.0.1:7126/api/students");
                if (data != null)
                {
                    var filtered = data.AsEnumerable();

                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        // Translate the searched Code (e.g. "CCE") into its full Department Name(s)
                        var matchingDeptNames = DepartmentsList
                            .Where(d => d.Code != null && d.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                            .Select(d => d.Name)
                            .ToList();

                        filtered = filtered.Where(s =>
                            (s.StudentName != null && s.StudentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (s.Email != null && s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (s.DepartmentName != null && s.DepartmentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (s.DepartmentName != null && matchingDeptNames.Contains(s.DepartmentName)) // Allows search by Code!
                        );
                    }

                    if (!string.IsNullOrEmpty(StatusFilter))
                    {
                        filtered = filtered.Where(s => s.Description != null && s.Description.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
                    }

                    var list = filtered.ToList();

                    // Calculate total pages
                    TotalPages = (int)Math.Ceiling(list.Count / (double)PageSize);
                    if (TotalPages == 0) TotalPages = 1;

                    // Ensure current page bounds are safe
                    if (CurrentPage > TotalPages) CurrentPage = TotalPages;
                    if (CurrentPage < 1) CurrentPage = 1;

                    // Sliding window calculation
                    VisitedStartPage = Math.Max(1, CurrentPage - 2);
                    VisitedEndPage = Math.Min(TotalPages, CurrentPage + 2);

                    StudentsList = list
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }
            }
            catch
            {
                StudentsList = new();
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var depts = await _httpClient.GetFromJsonAsync<List<Department>>("https://127.0.0.1:7126/api/departments");
                if (depts != null)
                {
                    DepartmentsList = depts;
                }
            }
            catch
            {
                DepartmentsList = new();
            }
        }

        private async Task LoadStatusesAsync()
        {
            try
            {
                var statuses = await _httpClient.GetFromJsonAsync<List<string>>("https://127.0.0.1:7126/api/students/statuses");
                if (statuses != null)
                {
                    StatusesList = statuses;
                }
            }
            catch
            {
                StatusesList = new();
            }
        }
    }
}