using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using Registration.Domain.Enums; // Make sure to include your Enums namespace!

namespace Registration.Web.Pages.Instructors
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public InstructorDetailsView? InstructorDetails { get; set; }

        // Dynamically grab all names from the Enum, and format "OnLeave" to "On Leave"
        public List<string> StatusesList { get; set; } = Enum.GetNames(typeof(InstructorStatus))
            .Select(status => status == "OnLeave" ? "On Leave" : status)
            .ToList();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var instructors = await _httpClient.GetFromJsonAsync<List<InstructorDetailsView>>($"https://127.0.0.1:7126/api/instructors?id={id}");
                InstructorDetails = instructors?.FirstOrDefault();
            }
            catch
            {
                InstructorDetails = null;
            }

            if (InstructorDetails == null)
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
                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/instructors/{id}/status", payload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Instructor status updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update instructor status.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while connecting to the server.";
            }

            return RedirectToPage(new { id = id });
        }
    }
}