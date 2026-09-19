namespace Registration.Domain
{
    public class GradeDetailsView
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty; // 🌟 ADD THIS PROPERTY

        public decimal? MidtermGrade { get; set; }
        public decimal? CourseWorkGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public decimal? TotalGrade { get; set; }
    }
}