using System;

namespace Registration.Domain
{
    public class SemesterCourseView
    {
        public int Id { get; set; } // The ID of the mapping in Courses_Semesters
        public int Course_Id { get; set; }
        public int Semester_Id { get; set; }

        // Joined from the Semesters table
        public string SemesterName { get; set; } = string.Empty;

        public string DepartmentCode { get; set; } = string.Empty;

        // Joined from the Courses table
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credit_Hours { get; set; }
        public int Difficulty { get; set; }
    }
}