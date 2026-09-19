namespace Registration.Domain
{
    public class InstructorCourseDetailsView
    {
        public int Id { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Semester_Id { get; set; } 
    }
}