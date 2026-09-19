
using Registration.Domain.Enums;

namespace Registration.Domain
{
    
    public class Instructor
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Name_AR { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public int? Start_year { get; set; }
        public string National_Id { get; set; }
        public float? Salary {  get; set; }

        public string? OfficeHours { get; set; }
        // 1. The Foreign Keys (Great for inserting/updating records in the database)
        public int? Department_Id { get; set; }
        public int? Status_Id { get; set; }

        // 2. The Navigation Objects (Great for reading and displaying rich data on the screen)
        public Department? Department { get; set; }
        public InstructorStatus? Status { get; set; }



    }
}
