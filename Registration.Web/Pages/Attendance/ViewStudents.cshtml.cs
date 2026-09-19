using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Registration.Web.Pages.Attendance
{
    public class ViewStudentsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViewStudentsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Using your existing domain model!
        public List<AttendanceDetailsView> StudentRoster { get; set; } = new();
        public DateTime SelectedDate { get; set; }
        public int ScheduleId { get; set; }

        public string CourseName { get; set; } = string.Empty;
        public string ScheduleDetails { get; set; } = string.Empty;

        public async Task OnGetAsync(int scheduleId, DateTime date)
        {
            ScheduleId = scheduleId;
            SelectedDate = date;

            try
            {
                // Make sure your API endpoint is set up to return a List<AttendanceDetailsView>
                var dateString = date.ToString("yyyy-MM-dd");
                // Change scheduleId to courseScheduleId and date to attendanceDate
                var apiUrl = $"https://127.0.0.1:7126/api/attendance/details?courseScheduleId={scheduleId}&attendanceDate={dateString}";

                var data = await _httpClient.GetFromJsonAsync<List<AttendanceDetailsView>>(apiUrl);

                if (data != null && data.Any())
                {
                    StudentRoster = data;

                    // Extract the course header info from the first record
                    var firstRecord = data.First();
                    CourseName = firstRecord.CourseName;
                    ScheduleDetails = $"{firstRecord.ScheduleDay} ({firstRecord.StartTime} - {firstRecord.EndTime})";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading attendance data: " + ex.Message;
            }
        }
    }
}