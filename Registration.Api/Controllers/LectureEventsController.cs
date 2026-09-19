using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess;
using Registration.Domain;
using System.Collections.Generic;
using System.Data;
using System;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LectureEventsController : ControllerBase
    {
        private readonly string _connectionString;

        public LectureEventsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        // ==========================================
        // 1. GET: api/LectureEvents/Course/{courseId}
        // ==========================================
        [HttpGet("Course/{courseId}")]
        public IActionResult GetPastLectures(int courseId)
        {
            var ops = new LectureEventOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    List<LectureDateView> results = ops.SelectByCourse(conn, courseId, out string message, out int status);

                    if (status == 1)
                    {
                        return Ok(results);
                    }

                    return BadRequest(new { message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
        }

        // ==========================================
        // 2. POST: api/LectureEvents (Creates a new lecture event session)
        // ==========================================
        [HttpPost]
        public IActionResult CreateLectureEvent([FromBody] LectureEvent newLecture)
        {
            if (newLecture == null)
            {
                return BadRequest(new { message = "Invalid lecture data." });
            }

            var ops = new LectureEventOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    bool isSuccess = ops.Create(conn, newLecture, out string message);

                    if (isSuccess)
                    {
                        return Ok(new { message = message });
                    }

                    return BadRequest(new { message = message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
        }

        // ==========================================
        // 3. PUT: api/LectureEvents/{id}
        // ==========================================
        [HttpPut("{id}")]
        public IActionResult UpdateLectureEvent(int id, [FromBody] LectureEvent updatedLecture)
        {
            if (updatedLecture == null)
            {
                return BadRequest(new { message = "Invalid update data." });
            }

            updatedLecture.Id = id;
            var ops = new LectureEventOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    bool isSuccess = ops.Update(conn, updatedLecture, out string message);

                    if (isSuccess)
                    {
                        return Ok(new { message = message });
                    }

                    return BadRequest(new { message = message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
        }

        // ==========================================
        // 4. DELETE: api/LectureEvents/{id}
        // ==========================================
        [HttpDelete("{id}")]
        public IActionResult DeleteLectureEvent(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID." });
            }

            var ops = new LectureEventOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    bool isSuccess = ops.Delete(conn, id, out string message);

                    if (isSuccess)
                    {
                        return Ok(new { message = message });
                    }

                    return BadRequest(new { message = message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
        }

        // ==========================================
        // 5. GET: api/LectureEvents/GetId (🌟 NEW: Fixes the redirect missing ID bug)
        // ==========================================
        [HttpGet("GetId")]
        public IActionResult GetLectureEventId([FromQuery] int scheduleId, [FromQuery] string date)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT TOP 1 Id FROM Lecture_Events WHERE Schedule_Id = @ScheduleId AND Date_Of_Event = @Date ORDER BY Id DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Parse(date).Date);

                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            return Ok(Convert.ToInt32(result));
                        }
                        return NotFound(new { message = "Session not found." });
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