using Registration.Domain.Enums;

namespace Registration.Domain
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
        public int? SemesterId { get; set; } // <-- Add this property here
        public EnrollmentStatus? StatusId { get; set; }
    }
}