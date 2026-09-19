using Registration.Domain.Enums;

namespace Registration.Domain
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Name_AR { get; set; }
        public string Code { get; set; }
        public int Credit_Hours { get; set; }
        public int? Difficulty { get; set; }

        // 1. The pure integer for the Foreign Key (Matches SQL perfectly)
        public int? Status_Id { get; set; }

        // 2. The Enum nametag for your C# application logic!
        public CourseStatus Status { get; set; }
    }
}
