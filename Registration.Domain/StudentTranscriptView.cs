namespace Registration.Domain
{
    public class StudentTranscriptView
    {
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public decimal? TotalGrade { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
    }
}