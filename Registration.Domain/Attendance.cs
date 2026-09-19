namespace Registration.Domain
{
    public class Attendance
    {
        public int Id { get; set; }
        public int LectureEvents_Id { get; set; }
        public int Student_Id { get; set; }

        // Added so the API can receive the list of checked student IDs during bulk-save
        public List<int> PresentStudentIds { get; set; } = new List<int>();
    }
}