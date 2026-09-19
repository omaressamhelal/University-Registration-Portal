using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System.Net.Http.Json;

namespace Registration.Web.Pages.Instructors
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<InstructorDetailsView> InstructorsList { get; set; } = new();
        public List<Department> DepartmentsList { get; set; } = new();
        public List<string> StatusesList { get; set; } = new();

        // Search & Pagination Properties
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        public int VisitedStartPage { get; set; }
        public int VisitedEndPage { get; set; }

        [BindProperty]
        public Instructor NewInstructor { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. LOAD DEPARTMENTS FIRST so the search can translate short codes!
            await LoadDepartmentsAsync();

            await LoadInstructorsAsync();
            await LoadStatusesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRegisterInstructorAsync()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/instructors", NewInstructor);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Instructor registered successfully!";
                    return RedirectToPage();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorContent) ? "Failed to register instructor." : errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Connection Error: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task LoadInstructorsAsync()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<InstructorDetailsView>>("https://127.0.0.1:7126/api/instructors");
                if (data != null)
                {
                    var filtered = data.AsEnumerable();

                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        // Translate the searched Code (e.g. "AEM") into its full Department Name
                        var matchingDeptNames = DepartmentsList
                            .Where(d => d.Code != null && d.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                            .Select(d => d.Name)
                            .ToList();

                        filtered = filtered.Where(i =>
                            (i.InstructorName != null && i.InstructorName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (i.Email != null && i.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (i.DepartmentName != null && i.DepartmentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (i.DepartmentName != null && matchingDeptNames.Contains(i.DepartmentName)) // Allows search by Code!
                        );
                    }

                    if (!string.IsNullOrEmpty(StatusFilter))
                    {
                        filtered = filtered.Where(i => i.StatusDescription != null && i.StatusDescription.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
                    }

                    var list = filtered.ToList();
                    TotalPages = (int)Math.Ceiling(list.Count / (double)PageSize);
                    if (TotalPages == 0) TotalPages = 1;

                    if (CurrentPage > TotalPages) CurrentPage = TotalPages;
                    if (CurrentPage < 1) CurrentPage = 1;

                    VisitedStartPage = Math.Max(1, CurrentPage - 2);
                    VisitedEndPage = Math.Min(TotalPages, CurrentPage + 2);

                    InstructorsList = list
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }
            }
            catch
            {
                InstructorsList = new();
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
                var statuses = await _httpClient.GetFromJsonAsync<List<string>>("https://127.0.0.1:7126/api/instructors/statuses");
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

        public async Task<IActionResult> OnPostUpdateInstructorAsync()
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/instructors/{NewInstructor.Id}", NewInstructor);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Instructor updated successfully!";
                    return RedirectToPage();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorContent) ? "Failed to update instructor." : errorContent;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Connection Error: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}