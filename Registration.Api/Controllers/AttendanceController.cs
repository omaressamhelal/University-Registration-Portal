using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.Domain;
using Registration.DataAccess;
using System;
using System.Collections.Generic;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly AttendanceOperations _attendanceOps;

        public AttendanceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _attendanceOps = new AttendanceOperations();
        }

        // ==========================================
        // 1. GET FUNCTION (Select Data / Admin View)
        // ==========================================
        [HttpGet("details")]
        public IActionResult GetAttendanceDetails([FromQuery] int? courseScheduleId, [FromQuery] string attendanceDate, [FromQuery] int? studentId)
        {
            AttendanceDetailsView searchCriteria = new AttendanceDetailsView();

            if (courseScheduleId.HasValue) searchCriteria.Course_Schedules_Id = courseScheduleId.Value;
            if (studentId.HasValue) searchCriteria.Student_Id = studentId.Value;

            DateTime parsedDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(attendanceDate) && DateTime.TryParse(attendanceDate, out parsedDate))
            {
                searchCriteria.AttendanceDate = parsedDate;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 🌟 SMART AUTO-HEAL: Ensure a Lecture_Events record exists for this schedule and date!
                if (courseScheduleId.HasValue && parsedDate != DateTime.MinValue)
                {
                    string checkLectureQuery = "SELECT Id FROM Lecture_Events WHERE Schedule_Id = @ScheduleId AND CAST(Date_Of_Event AS DATE) = CAST(@Date AS DATE)";
                    int lectureEventId = 0;

                    using (SqlCommand cmd = new SqlCommand(checkLectureQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ScheduleId", courseScheduleId.Value);
                        cmd.Parameters.AddWithValue("@Date", parsedDate.Date);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lectureEventId = Convert.ToInt32(result);
                        }
                    }

                    // If no lecture event exists for this date yet, create it automatically!
                    if (lectureEventId == 0)
                    {
                        string insertLectureQuery = "INSERT INTO Lecture_Events (Schedule_Id, Date_Of_Event, Content_Covered) VALUES (@ScheduleId, @Date, 'Regular Session')";
                        using (SqlCommand cmd = new SqlCommand(insertLectureQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@ScheduleId", courseScheduleId.Value);
                            cmd.Parameters.AddWithValue("@Date", parsedDate.Date);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Now fetch the student roster using your existing operations
                List<AttendanceDetailsView> results = _attendanceOps.SelectDetails(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(results);
                }
                else
                {
                    return BadRequest(new { message = message });
                }
            }
        }

        // ==========================================
        // 2. GET LECTURE DATES (For Dashboards)
        // ==========================================
        [HttpGet("lecture-dates")]
        public IActionResult GetLectureDates([FromQuery] int courseId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 🌟 Fixed to pass courseId matching AttendanceOperations.GetLectureDates
                var results = _attendanceOps.GetLectureDates(conn, courseId, out int status, out string message);

                if (status == 1)
                {
                    return Ok(results);
                }
                else
                {
                    return BadRequest(new { message = message });
                }
            }
        }

        // ==========================================
        // 3. POST FUNCTION (Direct ADO.NET Bulk Save)
        // ==========================================
        [HttpPost("bulk-save")]
        public IActionResult BulkSaveAttendance([FromBody] Attendance payload)
        {
            if (payload == null || payload.LectureEvents_Id <= 0)
            {
                return BadRequest(new { message = "Validation Failed: Invalid attendance data provided." });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. Clear existing attendance records for this lecture event directly
                    using (SqlCommand clearCmd = new SqlCommand("DELETE FROM Attendance WHERE LectureEvents_Id = @LectureEvents_Id", conn))
                    {
                        clearCmd.Parameters.AddWithValue("@LectureEvents_Id", payload.LectureEvents_Id);
                        clearCmd.ExecuteNonQuery();
                    }

                    // 2. Insert each checked student directly
                    int successCount = 0;
                    if (payload.PresentStudentIds != null && payload.PresentStudentIds.Count > 0)
                    {
                        foreach (int studentId in payload.PresentStudentIds)
                        {
                            using (SqlCommand insertCmd = new SqlCommand("INSERT INTO Attendance (LectureEvents_Id, Student_Id) VALUES (@LectureEvents_Id, @Student_Id)", conn))
                            {
                                insertCmd.Parameters.AddWithValue("@LectureEvents_Id", payload.LectureEvents_Id);
                                insertCmd.Parameters.AddWithValue("@Student_Id", studentId);
                                insertCmd.ExecuteNonQuery();
                            }
                            successCount++;
                        }
                    }

                    return Ok(new { message = $"Successfully saved {successCount} attendance records." });
                }
                catch (Exception ex)
                {
                    // Expose exact SQL error if anything fails
                    return BadRequest(new { message = $"Database Execution Error: {ex.Message}" });
                }
            }
        }

        // ==========================================
        // 4. DELETE FUNCTION (Remove single record)
        // ==========================================
        [HttpDelete("{id}")]
        public IActionResult DeleteAttendance(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Validation Failed: Invalid ID." });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = _attendanceOps.Delete(conn, id, out string message);

                if (isSuccess)
                {
                    return Ok(new { message = message });
                }
                else
                {
                    return BadRequest(new { message = message });
                }
            }
        }

        // ==========================================
        // 5. GET ATTENDANCE SHEET (Instructor View)
        // ==========================================
        [HttpGet]
        public IActionResult GetAttendanceSheet([FromQuery] int scheduleId, [FromQuery] int courseId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var results = _attendanceOps.GetInstructorAttendanceSheet(conn, courseId, scheduleId, out string message, out int status);

                if (status == 1)
                {
                    return Ok(results);
                }

                return BadRequest(new { message = message });
            }
        }
    }
}