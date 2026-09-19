using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.Departments
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<DepartmentDetailsView> DepartmentsList { get; set; } = new();

        [BindProperty]
        public Department Department { get; set; } = new();

        [BindProperty]
        public Department NewDepartment { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        // ==========================================
        // HANDLER: Proxy JSON to Client-Side Modal
        // ==========================================
        public async Task<IActionResult> OnGetDepartmentJsonAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Department>($"https://127.0.0.1:7126/api/departments/{id}");
                if (response == null)
                {
                    return NotFound();
                }
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Error loading department details: " + ex.Message);
            }
        }

        // ==========================================
        // HANDLER: Save Changes from Modal Form (Edit)
        // ==========================================
        public async Task<IActionResult> OnPostSaveModalAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed. Please check your inputs.";
                return RedirectToPage();
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:7126/api/departments/{Department.Id}", Department);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Department updated successfully!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "API Error: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving: " + ex.Message;
            }

            return RedirectToPage();
        }

        // ==========================================
        // HANDLER: Create New Department from Modal
        // ==========================================
        public async Task<IActionResult> OnPostCreateModalAsync()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://127.0.0.1:7126/api/departments", NewDepartment);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Department created successfully!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Failed to create department: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating: " + ex.Message;
            }

            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<DepartmentDetailsView>>("https://127.0.0.1:7126/api/departments");

                if (response != null)
                {
                    DepartmentsList = response;
                }
            }
            catch (Exception ex)
            {
                DepartmentsList = new List<DepartmentDetailsView>();
                TempData["ErrorMessage"] = "Could not connect to the Departments API: " + ex.Message;
            }
        }
    }
}