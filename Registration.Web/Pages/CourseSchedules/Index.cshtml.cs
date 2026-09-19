using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess;
using Registration.Domain;
using System;
using System.Collections.Generic;

namespace Registration.Web.Pages.CourseSchedules
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        [BindProperty(SupportsGet = true)]
        public string? CourseName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SemesterName { get; set; }

        public List<CourseScheduleDetailsView> SchedulesList { get; set; } = new();
        public SelectList? SemesterOptions { get; set; }
        public SelectList? InstructorCourseOptions { get; set; }

        // Form Binding for Creating a New Schedule
        [BindProperty]
        public int NewInstructorsCoursesId { get; set; }
        [BindProperty]
        public string NewDay { get; set; } = string.Empty;
        [BindProperty]
        public string NewType { get; set; } = string.Empty;
        [BindProperty]
        public TimeSpan NewStartTime { get; set; }
        [BindProperty]
        public TimeSpan NewEndTime { get; set; }

        public void OnGet()
        {
            LoadPageData();
        }

        public IActionResult OnPostCreate()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    INSERT INTO Course_Schedules (Instructors_Courses_Id, Day, Type, StartTime, EndTime) 
                    VALUES (@IcId, @Day, @Type, @Start, @End)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IcId", NewInstructorsCoursesId);
                    cmd.Parameters.AddWithValue("@Day", NewDay);
                    cmd.Parameters.AddWithValue("@Type", NewType);
                    cmd.Parameters.AddWithValue("@Start", NewStartTime);
                    cmd.Parameters.AddWithValue("@End", NewEndTime);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        TempData["SuccessMessage"] = "Course schedule created successfully!";
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Error creating schedule: " + ex.Message;
                    }
                }
            }

            return RedirectToPage(new { SemesterName = SemesterName });
        }

        private void LoadPageData()
        {
            var operations = new CourseScheduleOperations();
            var semesters = new List<string>();
            string latestSemester = string.Empty;

            var icList = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Fetch all semesters ordered by start date
                using (SqlCommand cmd = new SqlCommand("SELECT Name FROM Semesters ORDER BY Start_date DESC", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string semName = reader.GetString(0);
                            semesters.Add(semName);
                            if (string.IsNullOrEmpty(latestSemester)) latestSemester = semName;
                        }
                    }
                }

                // Default to latest semester if none provided
                if (string.IsNullOrEmpty(SemesterName) && !string.IsNullOrEmpty(latestSemester))
                {
                    SemesterName = latestSemester;
                }

                // 2. Fetch Schedules list based on filters
                SchedulesList = operations.Select(conn, CourseName, SemesterName, out int status, out string message);
                if (status == 0) TempData["ErrorMessage"] = message;

                // 3. Fetch Instructor-Course mappings filtered ONLY to the active/latest semester
                string icQuery = @"
                    SELECT ic.Id, c.Name + ' (' + c.Code + ') - ' + i.Name AS DisplayText
                    FROM Instructors_Courses ic
                    JOIN Courses c ON ic.Course_Id = c.Id
                    JOIN Instructors i ON ic.Instructor_Id = i.Id
                    JOIN Semesters s ON ic.Semester_Id = s.Id
                    WHERE s.Name = @SemName
                    ORDER BY c.Name";

                using (SqlCommand cmd = new SqlCommand(icQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@SemName", SemesterName ?? latestSemester);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            icList.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(0).ToString(),
                                Text = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            SemesterOptions = new SelectList(semesters, SemesterName);
            InstructorCourseOptions = new SelectList(icList, "Value", "Text");
        }
    }
}