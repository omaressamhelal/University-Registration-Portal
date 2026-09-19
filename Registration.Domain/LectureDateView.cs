namespace Registration.Domain
{
    public class LectureDateView
    {
        public int ScheduleId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int StudentsAttended { get; set; }

        // 🌟 Add this missing property so the dropdown can display the type (Lab/Lecture/Section)
        public string ScheduleType { get; set; } = string.Empty;
    }
}