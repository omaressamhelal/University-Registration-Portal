using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using Registration.Domain.Enums; // <--- Import your enums namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.Admins
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public AdminDetailsView AdminProfile { get; set; } = new();

        [BindProperty]
        public int NewStatusId { get; set; }

        // Helper class to hold dropdown options for the UI
        public class StatusOption
        {
            public int Id { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public List<StatusOption> AllStatusesList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid admin ID selected.";
                return RedirectToPage("/Admins/Index");
            }

            await LoadDataAsync();
            if (AdminProfile.Id == 0)
            {
                TempData["ErrorMessage"] = "Administrator not found.";
                return RedirectToPage("/Admins/Index");
            }

            return Page();
        }

        // ==========================================
        // HANDLER: Update Admin Status Only
        // ==========================================
        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            try
            {
                var updatePayload = new
                {
                    Status_id = NewStatusId
                };

                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/admins/{Id}", updatePayload);

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Admin status updated successfully!";
                else
                    TempData["ErrorMessage"] = "API Error: " + await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return Redirect(Request.Path + "?id=" + Id);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // 1. Get admin details profile from API
                var response = await _httpClient.GetFromJsonAsync<AdminDetailsView>($"https://127.0.0.1:7126/api/admins/{Id}");
                if (response != null)
                {
                    AdminProfile = response;
                    NewStatusId = AdminProfile.Status_id;
                }

                // 2. Populate status dropdown directly from your AdminStatus enum!
                AllStatusesList = Enum.GetValues(typeof(AdminStatus))
                    .Cast<AdminStatus>()
                    .Select(e => new StatusOption
                    {
                        Id = (int)e,
                        Description = e.ToString()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not load admin profile data: " + ex.Message;
            }
        }
    }
}