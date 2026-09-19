using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Registration.DataAccess;
using Registration.Domain;

namespace Registration.Web.Pages.Attendance
{
    public class LectureDatesModel : PageModel
    {
        private readonly string _connectionString;

        public LectureDatesModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public string CourseName { get; set; } = string.Empty;
        public string ScheduleType { get; set; } = string.Empty;
        public int TotalEnrolled { get; set; }
        public List<LectureDateView> DatesList { get; set; } = new();
        public int CurrentScheduleId { get; set; }

        public void OnGet(int scheduleId)
        {
            CurrentScheduleId = scheduleId;
            var operations = new AttendanceOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Fetch Course Name, Schedule Type, and Enrolled Count for this specific schedule
                string infoSql = @"
                    SELECT TOP 1
                        c.Name,
                        cs.Type,
                        (SELECT COUNT(*) FROM [Enrollments] e WHERE e.Course_Id = c.Id AND e.Status_Id = 1) AS TotalEnrolled
                    FROM [Course_Schedules] cs
                    JOIN [Instructors_Courses] ic ON cs.Instructors_Courses_Id = ic.Id
                    JOIN [Courses] c ON ic.Course_Id = c.Id
                    WHERE cs.Id = @ScheduleId";

                using (SqlCommand cmd = new SqlCommand(infoSql, conn))
                {
                    cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CourseName = reader.IsDBNull(0) ? "Unknown Course" : reader.GetString(0);
                            ScheduleType = reader.IsDBNull(1) ? "Session" : reader.GetString(1);
                            TotalEnrolled = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        }
                        else
                        {
                            CourseName = "Unknown Course";
                            ScheduleType = "Unknown";
                            TotalEnrolled = 0;
                        }
                    }
                }

                // 2. Fetch the attendance dates list for this schedule ID
                DatesList = operations.GetLectureDates(conn, scheduleId, out int status, out string message);

                if (status == 0)
                {
                    TempData["ErrorMessage"] = message;
                }
            }
        }
    }
}