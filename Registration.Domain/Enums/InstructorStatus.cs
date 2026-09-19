namespace Registration.Domain.Enums
{
    public enum InstructorStatus
    {
        Active = 1,
        Sabbatical = 2,
        PartTime = 3,  // Maps to 'Part-Time' in DB
        Resigned = 4,
        Retired = 5,
        OnLeave = 6,   // Maps to 'On Leave' in DB
        Pending = 7
    }
}