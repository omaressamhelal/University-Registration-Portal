public class EnrollmentDetailsView
{
    public int Id { get; set; }
    public int StudentId { get; set; }   
    public int CourseId { get; set; }    
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int? SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
}