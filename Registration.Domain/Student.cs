namespace Registration.Domain
{
    public class Student
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Name_AR { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public float? Gpa { get; set; }

        public int? Year { get; set; }

        public string National_Id { get; set; }

        // 1. Replaced the integer IDs with readable string codes
        public string DepartmentCode { get; set; }
        public string StatusDescription { get; set; }

        
    }
}