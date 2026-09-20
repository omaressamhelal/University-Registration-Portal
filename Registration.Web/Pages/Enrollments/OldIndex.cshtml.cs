using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using Registration.Domain.Enums;
using System.Text.Json;

namespace Registration.Web.Pages.Enrollments
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        // Properties for sliding window pagination
        public int VisitedStartPage { get; set; }
        public int VisitedEndPage { get; set; }

        public List<EnrollmentDetailsView> EnrollmentsList { get; set; } = new();
        public List<string> StatusesList { get; set; } = new();
        public List<StudentDetailsView> StudentsList { get; set; } = new();
        public List<CourseDetailsView> CoursesList { get; set; } = new();
        public List<Semester> SemestersList { get; set; } = new();

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task OnGetAsync()
        {
            StudentsList = await _httpClient.GetFromJsonAsync<List<StudentDetailsView>>("https://127.0.0.1:7126/api/students") ?? new();
            CoursesList = await _httpClient.GetFromJsonAsync<List<CourseDetailsView>>("https://127.0.0.1:7126/api/courses") ?? new();
            SemestersList = await _httpClient.GetFromJsonAsync<List<Semester>>("https://127.0.0.1:7126/api/semesters") ?? new();
            await LoadStatusesAsync();

            // Robust Default Logic: Open registration first, fallback to today's date, then newest semester
            if (!Request.Query.ContainsKey("SemesterFilter"))
            {
                // 1. Priority: Semester where registration is explicitly open
                var targetSemester = SemestersList.FirstOrDefault(s => s.Is_Registration_Open == true);

                if (targetSemester == null)
                {
                    // 2. Date Logic Fallback: The semester that is currently active today
                    var today = DateTime.Today;
                    targetSemester = SemestersList.FirstOrDefault(s => s.Start_date <= today && s.End_date >= today);
                }

                if (targetSemester == null)
                {
                    // 3. Final Safety Fallback: Just in case today's date falls in a gap between semesters
                    targetSemester = SemestersList.OrderByDescending(s => s.Start_date).FirstOrDefault();
                }

                if (targetSemester != null)
                {
                    SemesterFilter = targetSemester.Id;
                }
            }

            await LoadEnrollmentsAsync();
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SemesterFilter { get; set; }

        private async Task LoadEnrollmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://127.0.0.1:7126/api/enrollments");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var allEnrollments = JsonSerializer.Deserialize<List<EnrollmentDetailsView>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new();

                    // 1. Apply Search and Filters BEFORE pagination
                    var filtered = allEnrollments.AsEnumerable();

                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        filtered = filtered.Where(e =>
                            (e.StudentName != null && e.StudentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (e.CourseName != null && e.CourseName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                        );
                    }

                    if (!string.IsNullOrEmpty(StatusFilter))
                    {
                        filtered = filtered.Where(e => e.StatusName != null && e.StatusName.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
                    }

                    if (SemesterFilter.HasValue && SemesterFilter.Value > 0)
                    {
                        filtered = filtered.Where(e => e.SemesterId == SemesterFilter.Value);
                    }

                    var list = filtered.ToList();

                    // 2. Calculate pagination on the FILTERED list
                    int totalCount = list.Count;
                    TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
                    if (TotalPages == 0) TotalPages = 1;

                    if (CurrentPage > TotalPages) CurrentPage = TotalPages;
                    if (CurrentPage < 1) CurrentPage = 1;

                    VisitedStartPage = Math.Max(1, CurrentPage - 2);
                    VisitedEndPage = Math.Min(TotalPages, CurrentPage + 2);

                    // 3. Slice the exact 10 records for the current page
                    EnrollmentsList = list
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }
            }
            catch
            {
                EnrollmentsList = new();
            }
        }

        private async Task LoadStatusesAsync()
        {
            try
            {
                var statuses = await _httpClient.GetFromJsonAsync<List<string>>("https://127.0.0.1:7126/api/enrollments/statuses");
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

        public async Task<IActionResult> OnPostCreateAsync(int studentId, int courseId, string statusName)
        {
            if (Enum.TryParse<EnrollmentStatus>(statusName, out var parsedStatus))
            {
                // 🌟 Dynamically fetch semesters and apply the 3-step priority logic
                int targetSemesterId = 0;
                try
                {
                    var semesters = await _httpClient.GetFromJsonAsync<List<Semester>>("https://127.0.0.1:7126/api/semesters");
                    if (semesters != null && semesters.Any())
                    {
                        var targetSemester = semesters.FirstOrDefault(s => s.Is_Registration_Open == true);

                        if (targetSemester == null)
                        {
                            var today = DateTime.Today;
                            targetSemester = semesters.FirstOrDefault(s => s.Start_date <= today && s.End_date >= today);
                        }

                        if (targetSemester == null)
                        {
                            targetSemester = semesters.OrderByDescending(s => s.Start_date).FirstOrDefault();
                        }

                        if (targetSemester != null)
                        {
                            targetSemesterId = targetSemester.Id;
                        }
                    }
                }
                catch { }

                var newEnrollmentPayload = new
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    StatusId = parsedStatus,
                    SemesterId = targetSemesterId,
                    Semester_Id = targetSemesterId
                };

                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/enrollments", newEnrollmentPayload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "New enrollment created successfully in the active semester.";
                }
                else
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorDetails)
                        ? "Failed to create enrollment."
                        : errorDetails.Trim('"');
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid status selected.";
            }

            return RedirectToPage(new { SemesterFilter });
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string newStatus)
        {
            if (Enum.TryParse<EnrollmentStatus>(newStatus, out var parsedStatus))
            {
                var updatePayload = new
                {
                    Id = id,
                    StatusId = parsedStatus
                };

                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/enrollments/{id}", updatePayload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Enrollment status updated successfully.";
                }
                else
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorDetails)
                        ? "Failed to update enrollment status."
                        : errorDetails.Trim('"');
                }
            }

            return RedirectToPage(new { SemesterFilter });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"https://127.0.0.1:7126/api/enrollments/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Enrollment dropped successfully.";
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorDetails)
                    ? "Failed to drop enrollment."
                    : errorDetails.Trim('"');
            }

            return RedirectToPage(new { SemesterFilter });
        }
    }
}