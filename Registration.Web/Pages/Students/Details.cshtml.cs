using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using Registration.Domain.Enums;


namespace Registration.Web.Pages.Students
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public StudentDetailsView? StudentDetailsView { get; set; }

        // Automatically populates the dropdown list from your StudentStatus enum
        public List<string> StatusesList { get; set; } = Enum.GetNames(typeof(StudentStatus)).ToList();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var students = await _httpClient.GetFromJsonAsync<List<StudentDetailsView>>($"https://127.0.0.1:7126/api/students?id={id}");
                StudentDetailsView = students?.FirstOrDefault();
            }
            catch
            {
                StudentDetailsView = null;
            }

            if (StudentDetailsView == null)
            {
                return NotFound();
            }

            return Page();
        }
        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string newStatus)
        {
            try
            {
                var payload = new { NewStatus = newStatus };
                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/students/{id}/status", payload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Student status updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update student status.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while connecting to the student service.";
            }

            return RedirectToPage(new { id = id });
        }
    }
}