using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Registration.DataAccess;
using Registration.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly string _connectionString;

        public AdminsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetAdmins([FromQuery] int id = 0, [FromQuery] string name = null, [FromQuery] string email = null)
        {
            Admin searchCriteria = null;

            if (id > 0 || !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email))
            {
                searchCriteria = new Admin
                {
                    Id = id,
                    Name = name,
                    Email = email
                };
            }

            AdminOperations adminOps = new AdminOperations();
            List<AdminDetailsView> admins;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                admins = adminOps.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(admins);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // NEW: GET Admin By ID (Fixes the Details page issue)
        // ==========================================
        [HttpGet("{id}")]
        public IActionResult GetAdminById(int id)
        {
            AdminOperations adminOps = new AdminOperations();
            var searchCriteria = new Admin { Id = id };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var admins = adminOps.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1 && admins != null && admins.Count > 0)
                {
                    return Ok(admins[0]);
                }

                return NotFound(new { message = "Administrator not found." });
            }
        }

        [HttpPost]
        public IActionResult CreateAdmin([FromBody] Admin admin)
        {
            AdminOperations adminOps = new AdminOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = adminOps.Create(conn, admin, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAdmin(int id, [FromBody] Admin updatedAdmin)
        {
            updatedAdmin.Id = id;
            AdminOperations adminOps = new AdminOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = adminOps.Update(conn, updatedAdmin, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAdmin(int id)
        {
            AdminOperations adminOps = new AdminOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = adminOps.Delete(conn, id, out string message);

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
                string query = "SELECT Description FROM AdminsStatus";
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