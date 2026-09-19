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
    public class DepartmentsController : ControllerBase
    {
        private readonly string _connectionString;

        public DepartmentsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ==========================================
        // 1. GET ALL (Used for your list page)
        // ==========================================
        [HttpGet]
        public IActionResult GetDepartments([FromQuery] int id = 0, [FromQuery] string name = null, [FromQuery] string code = null)
        {
            Department searchCriteria = null;

            if (id > 0 || !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(code))
            {
                searchCriteria = new Department
                {
                    Id = id,
                    Name = name,
                    Code = code
                };
            }

            DepartmentOperations deptOps = new DepartmentOperations();
            List<DepartmentDetailsView> departments;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                departments = deptOps.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(departments);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 2. GET BY ID (Used for Editing Modal Popup)
        // ==========================================
        [HttpGet("{id}")]
        public IActionResult GetDepartmentById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid department ID.");
            }

            Department searchCriteria = new Department { Id = id };
            DepartmentOperations deptOps = new DepartmentOperations();
            List<DepartmentDetailsView> departments;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                departments = deptOps.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1 && departments != null && departments.Count > 0)
                {
                    return Ok(departments[0]);
                }

                return NotFound(message ?? "Department not found.");
            }
        }

        // ==========================================
        // 3. CREATE (INSERT)
        // ==========================================
        [HttpPost]
        public IActionResult CreateDepartment([FromBody] Department department)
        {
            if (department == null)
            {
                return BadRequest("Invalid department data.");
            }

            DepartmentOperations deptOps = new DepartmentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = deptOps.Create(conn, department, out string message);

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
        public IActionResult UpdateDepartment(int id, [FromBody] Department department)
        {
            if (department == null)
            {
                return BadRequest("Invalid department data.");
            }

            department.Id = id;

            DepartmentOperations deptOps = new DepartmentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = deptOps.Update(conn, department, out string message);

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
        public IActionResult DeleteDepartment(int id)
        {
            DepartmentOperations deptOps = new DepartmentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = deptOps.Delete(conn, id, out string message);

                if (success)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }
    }
}