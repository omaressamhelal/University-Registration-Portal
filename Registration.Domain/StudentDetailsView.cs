using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registration.Domain
{
    public class StudentDetailsView
    {
        public int Id { get; set; }
        public string StudentName { get; set; }

        public string? StudentName_AR { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public float Gpa { get; set; }
        public int Year { get; set; }
        public string National_Id { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string Description { get; set; } // Status description
        public List<string> MidtermGradesList { get; set; } = new List<string>();
        public List<string> CourseWorkGradesList { get; set; } = new List<string>();
        public List<string> FinalGradesList { get; set; } = new List<string>();
        public List<string> TotalGradesList { get; set; } = new List<string>();
        public List<string> EnrolledCoursesList { get; set; } = new List<string>();
        // Add this under your EnrolledCoursesList
        public List<string> EnrollmentStatusList { get; set; } = new List<string>();

        // Add this right under your EnrollmentStatusList
        public string CourseCodes { get; set; }
    }
}
