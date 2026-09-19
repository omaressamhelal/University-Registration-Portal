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
    public class SemestersController : ControllerBase
    {
        private readonly string _connectionString;

        public SemestersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ==========================================
        // 1. GET ALL (Used for your list page)
        // ==========================================
        [HttpGet]
        public IActionResult GetSemesters([FromQuery] int id = 0, [FromQuery] string name = null)
        {
            Semester searchCriteria = null;

            // Semesters don't have a code, so we just search by ID or Name
            if (id > 0 || !string.IsNullOrEmpty(name))
            {
                searchCriteria = new Semester
                {
                    Id = id,
                    Name = name ?? string.Empty
                };
            }

            SemesterOperations semOps = new SemesterOperations();
            List<Semester> semesters;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                semesters = semOps.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(semesters);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 2. GET BY ID (Used for Editing Modal Popup)
        // ==========================================
        [HttpGet("{id}")]
        public IActionResult GetSemesterById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid semester ID.");
            }

            Semester searchCriteria = new Semester { Id = id };
            SemesterOperations semOps = new SemesterOperations();
            List<Semester> semesters;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                semesters = semOps.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1 && semesters != null && semesters.Count > 0)
                {
                    return Ok(semesters[0]);
                }

                return NotFound(message ?? "Semester not found.");
            }
        }

        // ==========================================
        // 3. CREATE (INSERT)
        // ==========================================
        [HttpPost]
        public IActionResult CreateSemester([FromBody] Semester semester)
        {
            if (semester == null)
            {
                return BadRequest("Invalid semester data.");
            }

            SemesterOperations semOps = new SemesterOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = semOps.Create(conn, semester, out string message);

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
        public IActionResult UpdateSemester(int id, [FromBody] Semester semester)
        {
            if (semester == null)
            {
                return BadRequest("Invalid semester data.");
            }

            semester.Id = id;

            SemesterOperations semOps = new SemesterOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = semOps.Update(conn, semester, out string message);

                if (success)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 6. QUICK OPEN REGISTRATION
        // ==========================================
        [HttpPut("{id}/open-registration")]
        public IActionResult OpenRegistration(int id)
        {
            SemesterOperations semOps = new SemesterOperations();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = semOps.OpenRegistration(conn, id, out string message);
                if (success) return Ok(new { Message = message });
                return BadRequest(message);
            }
        }


        // ==========================================
        // 7. QUICK CLOSE REGISTRATION
        // ==========================================
        [HttpPut("{id}/close-registration")]
        public IActionResult CloseRegistration(int id)
        {
            SemesterOperations semOps = new SemesterOperations();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = semOps.CloseRegistration(conn, id, out string message);
                if (success) return Ok(new { Message = message });
                return BadRequest(message);
            }
        }


        // ==========================================
        // 5. DELETE
        // ==========================================
        [HttpDelete("{id}")]
        public IActionResult DeleteSemester(int id)
        {
            SemesterOperations semOps = new SemesterOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = semOps.Delete(conn, id, out string message);

                if (success)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }
    }
}