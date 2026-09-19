namespace Registration.Domain.Enums
{
    public enum CourseStatus
    {
        Available = 1,
        Closed = 2,
        UnderRevision = 3, // Maps to 'Under Revision' in DB
        Cancelled = 4,
        Archived = 5
    }
}