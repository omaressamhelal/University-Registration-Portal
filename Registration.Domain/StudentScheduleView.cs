namespace Registration.Domain
{
    public class StudentScheduleView
    {
        // 🌟 Add this property for student filtering
        public int StudentId { get; set; }

        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string Day { get; set; }
        public string Type { get; set; } // e.g., Lecture, Section, Lab
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string InstructorName { get; set; }
    }
}