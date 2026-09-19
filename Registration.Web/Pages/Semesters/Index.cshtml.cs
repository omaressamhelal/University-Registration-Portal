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
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<Semester> SemestersList { get; set; } = new();

        [BindProperty]
        public Semester Semester { get; set; } = new();

        [BindProperty]
        public Semester NewSemester { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        // ==========================================
        // HANDLER: Proxy JSON for Edit Modal
        // ==========================================
        public async Task<IActionResult> OnGetSemesterJsonAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Semester>($"https://127.0.0.1:7126/api/semesters/{id}");
                if (response == null) return NotFound();
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error loading semester details: " + ex.Message);
            }
        }

        // ==========================================
        // HANDLER: Save Changes (Update)
        // ==========================================
        public async Task<IActionResult> OnPostSaveModalAsync()
        {
            // Auto-handle Arabic name to keep UI clean
            Semester.Name_AR = Semester.Name;
            ModelState.Remove("Semester.Name_AR");

            // THE FIX: If checkbox was left unchecked (null), force it to false for the database
            Semester.Is_Registration_Open ??= false;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed. Please check your inputs.";
                return RedirectToPage();
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/semesters/{Semester.Id}", Semester);

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Semester updated successfully!";
                else
                    TempData["ErrorMessage"] = "API Error: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving: " + ex.Message;
            }

            return RedirectToPage();
        }

        // ==========================================
        // HANDLER: Create New Semester
        // ==========================================
        public async Task<IActionResult> OnPostCreateModalAsync()
        {
            // Auto-handle Arabic name to keep UI clean
            NewSemester.Name_AR = NewSemester.Name;
            ModelState.Remove("NewSemester.Name_AR");

            // THE FIX: If checkbox was left unchecked (null), force it to false for the database
            NewSemester.Is_Registration_Open ??= false;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/semesters", NewSemester);

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Semester created successfully!";
                else
                    TempData["ErrorMessage"] = "Failed to create semester: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating: " + ex.Message;
            }

            return RedirectToPage();
        }

        // ==========================================
        // HANDLER: Quick Toggle Open Registration
        // ==========================================
        public async Task<IActionResult> OnPostOpenRegistrationAsync(int id)
        {
            try
            {
                // We send a PUT request to our new custom endpoint. (null = no body required)
                var response = await _httpClient.PutAsync($"https://127.0.0.1:7126/api/semesters/{id}/open-registration", null);

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Registration opened successfully!";
                else
                    TempData["ErrorMessage"] = "Failed to open registration: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToPage();
        }


        // ==========================================
        // HANDLER: Quick Toggle Close Registration
        // ==========================================
        public async Task<IActionResult> OnPostCloseRegistrationAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"https://127.0.0.1:7126/api/semesters/{id}/close-registration", null);

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Registration closed successfully!";
                else
                    TempData["ErrorMessage"] = "Failed to close registration: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Semester>>("https://127.0.0.1:7126/api/semesters");
                if (response != null) SemestersList = response;
            }
            catch (Exception ex)
            {
                SemestersList = new List<Semester>();
                TempData["ErrorMessage"] = "Could not connect to the Semesters API: " + ex.Message;
            }
        }
    }
}