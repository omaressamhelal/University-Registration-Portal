namespace Registration.Domain
{
    public class CourseScheduleDetailsView
    {
        public int ScheduleId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public string SemesterName { get; set; }
        public string Day { get; set; }
        public string Type { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}