public class CourseDetailsView
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Name_AR { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Credit_Hours { get; set; }
    public int CreditHours { get => Credit_Hours; set => Credit_Hours = value; } // 🌟 Add this alias
    public int? Difficulty { get; set; }
    public int Status_Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Description_AR { get; set; }
    public string? DepartmentName { get; set; }
    public string? DepartmentCode { get; set; }
    public int TotalStudents { get; set; }
}