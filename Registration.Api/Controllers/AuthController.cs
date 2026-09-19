using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Registration.DataAccess;
using Registration.Domain;
using System.Data;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;

        public AuthController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 1. Check if the user is a Student
                var studentOperations = new StudentOperations();
                var searchCriteria = new Student { Email = request.Email };
                var studentsList = studentOperations.Select(conn, searchCriteria, out string message, out int status);
                var student = (studentsList != null && studentsList.Count > 0) ? studentsList[0] : null;

                if (student != null)
                {
                    if (student.Password.Trim() == request.Password.Trim())
                    {
                        // 🌟 FIX: Added the Id property
                        return Ok(new { Id = student.Id, Role = "Student", Name = student.StudentName });
                    }
                    else
                    {
                        return Unauthorized(new { message = "Incorrect password. Please try again." });
                    }
                }

                // 2. Check if the user is an Instructor
                var instructorOperations = new InstructorOperations();
                var instructorSearchCriteria = new Instructor { Email = request.Email };
                var instructorsList = instructorOperations.Select(conn, instructorSearchCriteria, out string instMessage, out int instStatus);
                var instructor = (instructorsList != null && instructorsList.Count > 0) ? instructorsList[0] : null;

                if (instructor != null)
                {
                    if (instructor.Password.Trim() == request.Password.Trim())
                    {
                        // 🌟 FIX: Added the Id property
                        return Ok(new { Id = instructor.Id, Role = "Instructor", Name = instructor.InstructorName });
                    }
                    else
                    {
                        return Unauthorized(new { message = "Incorrect password. Please try again." });
                    }
                }

                // 3. Check if the user is an Admin (Ensure connection is open since previous ops closed it)
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlCommand adminCmd = new SqlCommand("SELECT Id, Name, Password, Role FROM Admins WHERE Email = @Email", conn))
                {
                    adminCmd.Parameters.AddWithValue("@Email", request.Email);
                    using (SqlDataReader reader = adminCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string dbPassword = reader["Password"]?.ToString() ?? string.Empty;
                            if (dbPassword.Trim() == request.Password.Trim())
                            {
                                string adminName = reader["Name"]?.ToString() ?? "Admin";
                                // 🌟 FIX: Added the Id property from the SQL reader
                                return Ok(new { Id = Convert.ToInt32(reader["Id"]), Role = "Admin", Name = adminName });
                            }
                            else
                            {
                                return Unauthorized(new { message = "Incorrect password. Please try again." });
                            }
                        }
                    }
                }
            }

            // 4. Fallback: If none of the tables contain the email
            return Unauthorized(new { message = "Email not found. Please check your address or register." });
        }
    }
}