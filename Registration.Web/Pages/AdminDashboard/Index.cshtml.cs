using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System.Security.Claims;
using System.Text.Json;

namespace Registration.Web.Pages.AdminDashboard
{
    // Ensure only logged-in administrators can access this page
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        // Use the AdminDetailsView model we created!
        public AdminDetailsView AdminProfile { get; set; }

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task OnGetAsync()
        {
            // Grab the email from the authentication cookie
            var email = User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(email))
            {
                // Call your Admins API endpoint
                var response = await _httpClient.GetAsync($"https://127.0.0.1:7126/api/admins?email={email}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // Deserialize into a LIST of AdminDetailsView
                    var adminsList = JsonSerializer.Deserialize<List<AdminDetailsView>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Grab the first record
                    if (adminsList != null && adminsList.Count > 0)
                    {
                        AdminProfile = adminsList[0];
                    }
                }
            }
        }
    }
}