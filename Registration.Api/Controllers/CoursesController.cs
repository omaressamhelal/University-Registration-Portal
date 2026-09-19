using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Registration.Domain;
using Registration.DataAccess;
using System.Collections.Generic;

namespace Registration.Api.Controllers
{
    // The payload catches both the Course and the SelectedDepartmentId from the frontend
    public class CoursePayload
    {
        public Course Course { get; set; }
        public int DepartmentId { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly string _connectionString;

        public CoursesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetCourses([FromQuery] int id = 0, [FromQuery] string? name = null, [FromQuery] string? code = null)
        {
            Course searchCriteria = null;

            if (id > 0 || !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(code))
            {
                searchCriteria = new Course
                {
                    Id = id,
                    Name = name,
                    Code = code
                };
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                CourseOperations operations = new CourseOperations();

                List<CourseDetailsView> courses = operations.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(courses);
                }
                else
                {
                    return BadRequest(message);
                }
            }
        }

        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            var statuses = new List<string>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Description FROM CoursesStatus";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            statuses.Add(reader["Description"].ToString());
                        }
                    }
                }
            }
            return Ok(statuses);
        }

        [HttpPost]
        public IActionResult AddCourse([FromBody] CoursePayload payload)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                CourseOperations operations = new CourseOperations();

                // 🌟 FIX: Pass payload.DepartmentId into operations.Create
                bool isSuccess = operations.Create(conn, payload.Course, payload.DepartmentId, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }
                else
                {
                    return BadRequest(message);
                }
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCourse(int id, [FromBody] CoursePayload payload)
        {
            payload.Course.Id = id;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                CourseOperations operations = new CourseOperations();

                // 🌟 FIX: Pass payload.DepartmentId into operations.Update
                bool isSuccess = operations.Update(conn, payload.Course, payload.DepartmentId, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }
                else
                {
                    return BadRequest(message);
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCourse(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                CourseOperations operations = new CourseOperations();

                bool isSuccess = operations.Delete(conn, id, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }
                else
                {
                    return BadRequest(message);
                }
            }
        }
        // ==========================================
        // GET: api/courses/{id} (🌟 ADD THIS MISSING ENDPOINT)
        // ==========================================
        [HttpGet("{id}")]
        public IActionResult GetCourseById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid Course ID." });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                CourseOperations operations = new CourseOperations();

                // Search criteria filtered by the specific ID
                Course searchCriteria = new Course { Id = id };
                List<CourseDetailsView> courses = operations.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1 && courses != null && courses.Any())
                {
                    // Return the single course object directly
                    return Ok(courses.First());
                }

                return NotFound(new { message = string.IsNullOrEmpty(message) ? "Course not found." : message });
            }
        }
    }
}