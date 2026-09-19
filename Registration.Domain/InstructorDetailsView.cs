namespace Registration.Domain
{
    public class InstructorDetailsView
    {
        public int Id { get; set; }

        public string CourseIds { get; set; } = string.Empty;
        public string InstructorName { get; set; }
        public string InstructorName_AR { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string National_Id { get; set; }
        public int Start_year { get; set; }
        public float Salary { get; set; }

        // Flattened view columns
        public string DepartmentName { get; set; }

        public string ActiveSemesterName { get; set; } = "Fall 2026"; // Default or mapped from DB
        public string DepartmentCode { get; set; }
        public string StatusDescription { get; set; }
        public string TaughtCourses { get; set; }
        public string CourseCodes { get; set; }

        public string OfficeHours { get; set; } = string.Empty;

        public string ScheduleDays { get; set; } = string.Empty;
        public string ScheduleTimes { get; set; } = string.Empty;
        public string ScheduleTypes { get; set; } = string.Empty;

        public int? Department_Id { get; set; }
        public int? Status_Id { get; set; }
    }
}