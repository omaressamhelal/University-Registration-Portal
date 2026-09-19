using Microsoft.Data.SqlClient;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace Registration.DataAccess
{
    public class CourseScheduleOperations
    {
        public List<CourseScheduleDetailsView> Select(SqlConnection conn, string? courseName, string? semesterName, int? courseId, out int status, out string message)
        {
            var list = new List<CourseScheduleDetailsView>();
            status = 1;
            message = "Success";

            string query = "SELECT * FROM vw_CourseScheduleDetails WHERE 1=1";

            if (!string.IsNullOrEmpty(courseName)) query += " AND CourseName LIKE @CourseName";
            if (!string.IsNullOrEmpty(semesterName)) query += " AND SemesterName LIKE @SemesterName";
            if (courseId.HasValue && courseId.Value > 0) query += " AND Course_Id = @CourseId"; // Assumes vw_CourseScheduleDetails has Course_Id

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (!string.IsNullOrEmpty(courseName)) cmd.Parameters.AddWithValue("@CourseName", $"%{courseName}%");
                if (!string.IsNullOrEmpty(semesterName)) cmd.Parameters.AddWithValue("@SemesterName", $"%{semesterName}%");
                if (courseId.HasValue && courseId.Value > 0) cmd.Parameters.AddWithValue("@CourseId", courseId.Value);

                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CourseScheduleDetailsView
                            {
                                ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                                CourseCode = reader.GetString(reader.GetOrdinal("CourseCode")),
                                CourseName = reader.GetString(reader.GetOrdinal("CourseName")),
                                InstructorName = reader.GetString(reader.GetOrdinal("InstructorName")),
                                SemesterName = reader.GetString(reader.GetOrdinal("SemesterName")),
                                Day = reader.GetString(reader.GetOrdinal("Day")),
                                Type = reader.GetString(reader.GetOrdinal("Type")),
                                StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime"))
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    status = 0;
                    message = ex.Message;
                }
            }
            return list;
        }

        // Backward compatibility overload
        public List<CourseScheduleDetailsView> Select(SqlConnection conn, string? courseName, string? semesterName, out int status, out string message)
        {
            return Select(conn, courseName, semesterName, null, out status, out message);
        }
    }
}