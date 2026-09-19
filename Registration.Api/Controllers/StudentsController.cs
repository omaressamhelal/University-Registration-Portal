using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Registration.Domain;
using Registration.DataAccess;
using System.Text.Json.Serialization;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly string _connectionString;

        public StudentsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        [HttpGet]
        public IActionResult GetStudents([FromQuery] int id = 0, [FromQuery] string name = null, [FromQuery] string email = null)
        {
            // 1. Build the search criteria object only if the user actually provided search terms
            Student searchCriteria = null;

            if (id > 0 || !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email))
            {
                searchCriteria = new Student
                {
                    Id = id,
                    Name = name,
                    Email = email
                };
            }

            // 2. Prepare variables to catch the outputs from your Select method
            StudentOperations studentOps = new StudentOperations();
            List<StudentDetailsView> students;

            // 3. Open connection and call the database
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Call your Select method
                students = studentOps.Select(conn, searchCriteria, out string message, out int status);

                // 4. Check the status flag returned by your method
                if (status == 1)
                {
                    // Returns a 200 OK along with the actual JSON list of students
                    return Ok(students);
                }

                // If status is 0 (an exception happened), return a 400 Bad Request with the error
                return BadRequest(message);
            }
        }

        [HttpPost]
        public IActionResult CreateStudent([FromBody] Student student)
        {
            StudentOperations studentOps = new StudentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Passes the connection, the student model, and an out parameter for the message
                bool isSuccess = studentOps.Create(conn, student, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                // If validation or database constraints fail, return the 400 with the error message
                return BadRequest(message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id, [FromBody] Student updatedStudent)
        {
            // Ensure the ID in the URL matches the ID in the body, or assign it directly
            updatedStudent.Id = id;

            StudentOperations studentOps = new StudentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = studentOps.Update(conn, updatedStudent, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            StudentOperations studentOps = new StudentOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = studentOps.Delete(conn, id, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        public class StatusUpdateModel
        {
            [JsonPropertyName("newStatus")]
            public string NewStatus { get; set; }
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.NewStatus))
            {
                return BadRequest("Payload is null.");
            }

            StudentOperations studentOps = new StudentOperations();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = studentOps.UpdateStatusOnly(conn, id, model.NewStatus, out string message);
                if (success)
                {
                    return Ok(message);
                }
                // Return the actual SQL or exception error message to your browser/debugger
                return BadRequest(message);
            }
        }


        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            var statuses = new List<string>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Description FROM StudentsStatus"; // Adjust table name if necessary
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