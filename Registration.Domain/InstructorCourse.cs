namespace Registration.Domain
{
    public class InstructorCourse
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public int CourseId { get; set; }
        public int Semester_Id { get; set; } 
    }
}