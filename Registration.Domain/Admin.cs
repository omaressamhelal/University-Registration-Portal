using Registration.Domain.Enums;

namespace Registration.Domain
{
    public class Admin
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Name_AR { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? National_Id { get; set; }
        public int? Start_year { get; set; }
        public float Salary { get; set; }
        public string Role { get; set; } = string.Empty;

        // Foreign Key
        public int Status_Id { get; set; }

        // Navigation Enum Property
        public AdminStatus Status { get; set; }
    }
}