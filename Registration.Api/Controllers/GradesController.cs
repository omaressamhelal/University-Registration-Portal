using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Registration.Domain;
using Registration.DataAccess;
using System.Collections.Generic;

namespace Registration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly string _connectionString;

        public GradesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        // ==========================================
        // 1. GET GRADES BY COURSE
        // ==========================================
        [HttpGet("Course/{courseId:int}")]
        public IActionResult GetGradesByCourse(int courseId)
        {
            GradeOperations gradeOps = new GradeOperations();
            List<GradeDetailsView> grades;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                grades = gradeOps.GetGradesByCourse(conn, courseId, out string message, out int status);

                if (status == 1)
                {
                    return Ok(grades);
                }

                return BadRequest(message);
            }
        }

        // ==========================================
        // 2. SAVE OR UPDATE GRADES (UPSERT)
        // ==========================================
        [HttpPost]
        public IActionResult SaveGrade([FromBody] Grade gradeModel)
        {
            if (gradeModel == null || gradeModel.Enrollment_Id <= 0)
            {
                return BadRequest("Validation Failed: Invalid grade submission payload.");
            }

            GradeOperations gradeOps = new GradeOperations();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                bool isSuccess = gradeOps.SaveGrade(
                    conn,
                    gradeModel.Enrollment_Id,
                    gradeModel.Midterm_Grade,
                    gradeModel.CourseWork_Grade,
                    gradeModel.Final_Grade,
                    out string message
                );

                if (isSuccess)
                {
                    return Ok(message);
                }

                return BadRequest(message);
            }
        }
    }
}