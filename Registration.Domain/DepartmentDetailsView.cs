namespace Registration.Domain
{
    public class DepartmentDetailsView
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Name_AR { get; set; }
        public string Code { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public decimal AverageGpa { get; set; }
    }
}