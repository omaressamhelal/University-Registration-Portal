using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess_;
using Registration.Domain;
using System.Collections.Generic;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorCoursesController : ControllerBase
    {
        private readonly string _connectionString;

        public InstructorCoursesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ==========================================
        // 1. GET: api/instructorcourses (Supports optional search criteria via query string)
        // ==========================================
        [HttpGet]
        public IActionResult GetInstructorCourses(
            [FromQuery] int id = 0,
            [FromQuery] int? instructorId = null,
            [FromQuery] int? courseId = null)
        {
            InstructorCourse searchCriteria = new InstructorCourse
            {
                Id = id,
                InstructorId = instructorId ?? 0,
                CourseId = courseId ?? 0
            };

            InstructorCourseOperations ops = new InstructorCourseOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                List<InstructorCourseDetailsView> results = ops.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(results);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 2. POST: api/instructorcourses (Creates a new assignment)
        // ==========================================
        [HttpPost]
        public IActionResult CreateInstructorCourse([FromBody] InstructorCourse newAssignment)
        {
            InstructorCourseOperations ops = new InstructorCourseOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = ops.Create(conn, newAssignment, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 3. PUT: api/instructorcourses/{id} (Updates an existing assignment)
        // ==========================================
        [HttpPut("{id}")]
        public IActionResult UpdateInstructorCourse(int id, [FromBody] InstructorCourse updatedAssignment)
        {
            updatedAssignment.Id = id;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Fetch existing record's foreign keys including Semester_Id so partial updates don't wipe them
                using (SqlCommand selectCmd = new SqlCommand("SELECT Instructor_id, Course_id, Semester_Id FROM Instructors_Courses WHERE Id = @Id", conn))
                {
                    selectCmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (updatedAssignment.InstructorId == 0) updatedAssignment.InstructorId = reader.GetInt32(0);
                            if (updatedAssignment.CourseId == 0) updatedAssignment.CourseId = reader.GetInt32(1);
                            if (updatedAssignment.Semester_Id == 0) updatedAssignment.Semester_Id = reader.GetInt32(2); // 🌟 Preserves Semester_Id
                        }
                        else
                        {
                            return NotFound("Instructor course assignment record not found.");
                        }
                    }
                }

                // 2. Perform the update operation
                InstructorCourseOperations ops = new InstructorCourseOperations();
                bool isSuccess = ops.Update(conn, updatedAssignment, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 4. DELETE: api/instructorcourses/{id} (Removes an assignment)
        // ==========================================
        [HttpDelete("{id}")]
        public IActionResult DeleteInstructorCourse(int id)
        {
            InstructorCourseOperations ops = new InstructorCourseOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = ops.Delete(conn, id, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }
    }
}