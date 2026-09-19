using System;

namespace Registration.Domain
{
    public class AttendanceDetailsView
    {
        public int Id { get; set; }
        public int LectureEvents_Id { get; set; }
        public int Course_Schedules_Id { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int Student_Id { get; set; }

        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string ScheduleDay { get; set; } = string.Empty;
        public string ScheduleType { get; set; } = string.Empty;
        public string Content_Covered { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // 🌟 Fixed: changed -> to =>
        public bool IsPresent => Id > 0;
    }
}