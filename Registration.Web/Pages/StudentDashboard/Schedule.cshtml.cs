using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Registration.DataAccess;
using Registration.Domain;

namespace Registration.Web.Pages.StudentDashboard
{
    public class ScheduleModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // 🌟 Expose StudentId as a public bound property so the Razor view can access it
        [BindProperty(SupportsGet = true)]
        public int StudentId { get; set; }

        public List<StudentScheduleView> WeeklySchedule { get; set; } = new List<StudentScheduleView>();
        public string ErrorMessage { get; set; } = string.Empty;

        public ScheduleModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            if (StudentId <= 0)
            {
                // Fallback for testing - replace with actual logged-in user logic later
                StudentId = 1;
            }

            StudentOperations ops = new StudentOperations();
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                WeeklySchedule = ops.GetStudentSchedule(conn, StudentId, out string msg);
                ErrorMessage = msg;
            }
        }
    }
}