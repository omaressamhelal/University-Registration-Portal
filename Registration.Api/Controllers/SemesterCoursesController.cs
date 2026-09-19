using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.Domain;
using Registration.DataAccess;
using System.Collections.Generic;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemesterCoursesController : ControllerBase
    {
        private readonly string _connectionString;

        public SemesterCoursesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ==========================================
        // 1. GET ALL / FILTER BY SEMESTER OR COURSE
        // ==========================================
        [HttpGet]
        public IActionResult GetSemesterCourses([FromQuery] int id = 0, [FromQuery] int semesterId = 0, [FromQuery] int courseId = 0)
        {
            SemesterCourseView searchCriteria = null;

            if (id > 0 || semesterId > 0 || courseId > 0)
            {
                searchCriteria = new SemesterCourseView
                {
                    Id = id,
                    Semester_Id = semesterId,
                    Course_Id = courseId
                };
            }

            SemesterCourseOperations ops = new SemesterCourseOperations();
            List<SemesterCourseView> assignments;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                assignments = ops.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(assignments);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 2. GET BY ID
        // ==========================================
        [HttpGet("{id}")]
        public IActionResult GetSemesterCourseById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid assignment ID.");
            }

            SemesterCourseView searchCriteria = new SemesterCourseView { Id = id };
            SemesterCourseOperations ops = new SemesterCourseOperations();
            List<SemesterCourseView> assignments;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                assignments = ops.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1 && assignments != null && assignments.Count > 0)
                {
                    return Ok(assignments[0]);
                }

                return NotFound(message ?? "Assignment not found.");
            }
        }

        // ==========================================
        // 3. CREATE (INSERT)
        // ==========================================
        [HttpPost]
        public IActionResult CreateSemesterCourse([FromBody] SemesterCourseView assignment)
        {
            if (assignment == null)
            {
                return BadRequest("Invalid assignment data.");
            }

            SemesterCourseOperations ops = new SemesterCourseOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = ops.Create(conn, assignment, out string message);

                if (success)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 4. UPDATE
        // ==========================================
        [HttpPut("{id}")]
        public IActionResult UpdateSemesterCourse(int id, [FromBody] SemesterCourseView assignment)
        {
            if (assignment == null)
            {
                return BadRequest("Invalid assignment data.");
            }

            assignment.Id = id;

            SemesterCourseOperations ops = new SemesterCourseOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = ops.Update(conn, assignment, out string message);

                if (success)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 5. DELETE
        // ==========================================
        [HttpDelete("{id}")]
        public IActionResult DeleteSemesterCourse(int id)
        {
            SemesterCourseOperations ops = new SemesterCourseOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = ops.Delete(conn, id, out string message);

                if (success)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }
    }
}