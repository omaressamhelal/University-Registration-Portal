using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Registration.DataAccess;
using Registration.Domain;
using static Registration.Api.Controllers.StudentsController;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly string _connectionString;

        public InstructorsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetInstructors([FromQuery] int id = 0, [FromQuery] string name = null, [FromQuery] string email = null)
        {
            Instructor searchCriteria = null;

            if (id > 0 || !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email))
            {
                searchCriteria = new Instructor
                {
                    Id = id,
                    Name = name,
                    Email = email
                };
            }

            InstructorOperations instructorOps = new InstructorOperations();
            List<InstructorDetailsView> instructors;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                instructors = instructorOps.Select(conn, searchCriteria, out string message, out int status);

                if (status == 1)
                {
                    return Ok(instructors);
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
                string query = "SELECT Description FROM InstructorsStatus";
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
        public IActionResult CreateInstructor([FromBody] Instructor instructor)
        {
            InstructorOperations instructorOps = new InstructorOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = instructorOps.Create(conn, instructor, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateInstructor(int id, [FromBody] Instructor updatedInstructor)
        {
            updatedInstructor.Id = id;

            InstructorOperations instructorOps = new InstructorOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = instructorOps.Update(conn, updatedInstructor, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteInstructor(int id)
        {
            InstructorOperations instructorOps = new InstructorOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = instructorOps.Delete(conn, id, out string message);

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.NewStatus))
            {
                return BadRequest("Payload is null.");
            }

            InstructorOperations instructorOps = new InstructorOperations();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool success = instructorOps.UpdateStatusOnly(conn, id, model.NewStatus, out string message);
                if (success)
                {
                    return Ok(message);
                }
                return BadRequest(message);
            }
        }
    }
}