using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Registration.Domain;
using Registration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.Admins
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<AdminDetailsView> AdminsList { get; set; } = new();

        [BindProperty]
        public Admin NewAdmin { get; set; } = new();

        [BindProperty]
        public Admin Admin { get; set; } = new();

        // Status option holder for the dropdown
        public class StatusOption
        {
            public int Id { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public List<StatusOption> AllStatusesList { get; set; } = new();

        // 🌟 Convenient SelectList for Razor HTML dropdown rendering
        public SelectList StatusesSelectList => new SelectList(AllStatusesList, "Id", "Description", Admin.Status_Id);

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetAdminJsonAsync(int id)
        {
            try
            {
                var admin = await _httpClient.GetFromJsonAsync<Admin>($"https://127.0.0.1:7126/api/admins/{id}");
                if (admin != null)
                {
                    return new JsonResult(admin);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return NotFound();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/admins", NewAdmin);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Administrator created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create administrator: " + await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveModalAsync()
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/admins/{Admin.Id}", Admin);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Administrator details updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update administrator: " + await response.Content.ReadAsStringAsync();
                }
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
                // 1. Load Admins List
                var response = await _httpClient.GetFromJsonAsync<List<AdminDetailsView>>("https://127.0.0.1:7126/api/admins");
                if (response != null)
                {
                    AdminsList = response;
                }

                // 2. Load Statuses from API database table [AdminsStatus]
                try
                {
                    var statusResponse = await _httpClient.GetFromJsonAsync<List<StatusOption>>("https://127.0.0.1:7126/api/admins/statuses");
                    if (statusResponse != null && statusResponse.Any())
                    {
                        AllStatusesList = statusResponse;
                    }
                }
                catch
                {
                    // Fallback to Enum if API endpoint isn't created yet
                    AllStatusesList = Enum.GetValues(typeof(AdminStatus))
                        .Cast<AdminStatus>()
                        .Select(e => new StatusOption
                        {
                            Id = (int)e,
                            Description = e.ToString()
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not connect to API to load data: " + ex.Message;
            }
        }
    }
}