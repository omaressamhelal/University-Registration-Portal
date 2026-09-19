using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Registration.DataAccess_;
using Registration.Domain;
using Registration.Domain.Enums;
using System.Collections.Generic;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly string _connectionString;

        public EnrollmentsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ==========================================
        // 1. GET: api/enrollments (Supports optional search criteria via query string)
        // ==========================================
        [HttpGet]
        public IActionResult GetEnrollments(
            [FromQuery] int id = 0,
            [FromQuery] int? studentId = null,
            [FromQuery] int? courseId = null,
            [FromQuery] EnrollmentStatus? statusId = null)
        {
            Enrollment searchCriteria = new Enrollment
            {
                Id = id,
                StudentId = studentId,
                CourseId = courseId,
                StatusId = statusId
            };

            EnrollmentOperations ops = new EnrollmentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                List<EnrollmentDetailsView> results = ops.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(results);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 2. POST: api/enrollments (Creates a new enrollment)
        // ==========================================
        [HttpPost]
        public IActionResult CreateEnrollment([FromBody] Enrollment newEnrollment)
        {
            EnrollmentOperations ops = new EnrollmentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = ops.Create(conn, newEnrollment, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 3. PUT: api/enrollments/{id} (Updates an existing enrollment)
        // ==========================================
        [HttpPut("{id}")]
        public IActionResult UpdateEnrollment(int id, [FromBody] Enrollment updatedEnrollment)
        {
            updatedEnrollment.Id = id;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Fetch the existing record's foreign keys so we don't lose them
                using (SqlCommand selectCmd = new SqlCommand("SELECT Student_Id, Course_Id, Semester_Id FROM Enrollments WHERE Id = @Id", conn))
                {
                    selectCmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            updatedEnrollment.StudentId = reader.GetInt32(0);
                            updatedEnrollment.CourseId = reader.GetInt32(1);
                            updatedEnrollment.SemesterId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
                        }
                        else
                        {
                            return NotFound("Enrollment record not found.");
                        }
                    }
                }

                // 2. Perform the update operation
                EnrollmentOperations ops = new EnrollmentOperations();
                bool isSuccess = ops.Update(conn, updatedEnrollment, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 4. DELETE: api/enrollments/{id} (Removes an enrollment)
        // ==========================================
        [HttpDelete("{id}")]
        public IActionResult DeleteEnrollment(int id)
        {
            EnrollmentOperations ops = new EnrollmentOperations();

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

        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            var statuses = new List<string>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Description FROM EnrollmentsStatus";
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
    }
}