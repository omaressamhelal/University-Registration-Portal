using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Registration.Domain;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseSchedulesController : ControllerBase
    {
        private readonly string _connectionString;

        public CourseSchedulesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        // ==========================================
        // 1. GET: api/CourseSchedules/Course/{courseId}
        // ==========================================
        [HttpGet("Course/{courseId:int}")]
        public IActionResult GetSchedulesByCourse(int courseId)
        {
            var schedules = new List<CourseScheduleDetailsView>();
            string query = @"
                SELECT 
                    cs.Id AS ScheduleId,
                    cs.Day,
                    cs.Type,
                    cs.StartTime,
                    cs.EndTime,
                    c.Code AS CourseCode,
                    c.Name AS CourseName
                FROM Course_Schedules cs
                INNER JOIN Instructors_Courses ic ON cs.Instructors_Courses_Id = ic.Id
                INNER JOIN Courses c ON ic.Course_Id = c.Id
                WHERE ic.Course_Id = @CourseId";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                schedules.Add(new CourseScheduleDetailsView
                                {
                                    ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                                    Day = reader.IsDBNull(reader.GetOrdinal("Day")) ? string.Empty : reader.GetString(reader.GetOrdinal("Day")),
                                    Type = reader.IsDBNull(reader.GetOrdinal("Type")) ? string.Empty : reader.GetString(reader.GetOrdinal("Type")),
                                    StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? TimeSpan.Zero : reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? TimeSpan.Zero : reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                                    CourseCode = reader.IsDBNull(reader.GetOrdinal("CourseCode")) ? string.Empty : reader.GetString(reader.GetOrdinal("CourseCode")),
                                    CourseName = reader.IsDBNull(reader.GetOrdinal("CourseName")) ? string.Empty : reader.GetString(reader.GetOrdinal("CourseName"))
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { message = ex.Message });
                    }
                }
            }
            return Ok(schedules);
        }

        // ==========================================
        // 2. GET: api/CourseSchedules/GetOrCreate/{courseId}
        // 🌟 SMART AUTO-HEAL: Creates a schedule for the selected type!
        // ==========================================
        [HttpGet("GetOrCreate/{courseId:int}")]
        public IActionResult GetOrCreateSchedule(int courseId, [FromQuery] string type = "Lecture")
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. Try to find an existing schedule for this specific type (Lecture, Lab, etc.)
                    string findQuery = @"
                        SELECT TOP 1 cs.Id 
                        FROM Course_Schedules cs
                        INNER JOIN Instructors_Courses ic ON cs.Instructors_Courses_Id = ic.Id
                        WHERE ic.Course_Id = @CourseId AND cs.Type = @Type";

                    using (SqlCommand cmd = new SqlCommand(findQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@Type", type);
                        var result = cmd.ExecuteScalar();
                        if (result != null) return Ok(Convert.ToInt32(result)); // Found it!
                    }

                    // 2. If missing, find the Instructors_Courses_Id so we can create one
                    string findIcQuery = "SELECT TOP 1 Id FROM Instructors_Courses WHERE Course_Id = @CourseId";
                    int icId = 0;
                    using (SqlCommand cmd = new SqlCommand(findIcQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        var result = cmd.ExecuteScalar();
                        if (result == null) return NotFound(new { message = "Course not assigned to any instructor." });
                        icId = Convert.ToInt32(result);
                    }

                    // 3. Generate a default schedule in the database for the selected type
                    string insertQuery = @"
                        INSERT INTO Course_Schedules (Instructors_Courses_Id, Day, Type, StartTime, EndTime)
                        OUTPUT INSERTED.Id
                        VALUES (@IcId, 'Sunday', @Type, '09:00:00', '11:00:00')";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@IcId", icId);
                        cmd.Parameters.AddWithValue("@Type", type);
                        var newId = cmd.ExecuteScalar();
                        return Ok(Convert.ToInt32(newId)); // Success! Auto-healed.
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
        }
    }
}