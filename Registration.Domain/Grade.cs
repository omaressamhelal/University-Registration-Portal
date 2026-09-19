namespace Registration.Domain
{
    public class Grade
    {
        public int Id { get; set; }
        public int Enrollment_Id { get; set; }

        // We use nullable decimals (decimal?) because a student might not have a grade yet
        public decimal? Midterm_Grade { get; set; }
        public decimal? CourseWork_Grade { get; set; }
        public decimal? Final_Grade { get; set; }

        // This is computed automatically by the database!
        public decimal? Total_Grade { get; set; }
    }
}