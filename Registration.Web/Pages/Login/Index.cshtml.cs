using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Net.Http.Json;

namespace Registration.Web.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            // Check if the user already has an active login cookie
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // CORRECTED ROUTING: Send users to their proper dashboards
                if (role == "Admin")
                {
                    return RedirectToPage("/AdminDashboard/Index");
                }
                else if (role == "Instructor")
                {
                    return RedirectToPage("/InstructorDashboard/Index");
                }

                // 🌟 FIX: We must pass the studentId from the cookie back to the URL
                var userId = User.FindFirst("UserId")?.Value;
                return RedirectToPage("/StudentDashboard/Index", new { studentId = userId });
            }

            // If they are NOT logged in, show the login page normally
            return Page();
        }

        public class ApiLoginResponse
        {
            // 🌟 CRITICAL FIX: We MUST capture the ID from your API to issue the security claim
            public int Id { get; set; }
            public string Role { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var loginData = new { Email = this.Email, Password = this.Password };
            var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid response from server.");
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Email),
                    new Claim("FullName", result.Name),
                    new Claim(ClaimTypes.Role, result.Role),
                    // 🌟 CRITICAL FIX: This is the missing ID card the Degree Plan page was looking for!
                    new Claim("UserId", result.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // --- NEW COOKIE LOGIC ---
                var authProperties = new AuthenticationProperties
                {
                    // Setting it to false means the cookie deletes when the browser closes.
                    IsPersistent = false,
                    // Automatically log them out if they are inactive for 60 minutes
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                // Sign in using the new authProperties
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // --- ROUTING LOGIC ---
                if (result.Role == "Student")
                {
                    // 🌟 FIX: Must pass the ID into the URL so the dashboard knows who to load!
                    return RedirectToPage("/StudentDashboard/Index", new { studentId = result.Id });
                }
                else if (result.Role == "Admin")
                {
                    return RedirectToPage("/AdminDashboard/Index");
                }
                else
                {
                    return RedirectToPage("/InstructorDashboard/Index");
                }
            }
            else
            {
                // If login fails, extract the error and reload the page
                var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                string errorMessage = errorResponse != null && errorResponse.ContainsKey("message")
                    ? errorResponse["message"]
                    : "Invalid login attempt.";

                ModelState.AddModelError(string.Empty, errorMessage);

                return Page(); // Returns the current page so the user sees the error
            }
        }
    }
}