using Microsoft.Data.SqlClient;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace Registration.DataAccess
{
    public class AttendanceOperations
    {
        // ==========================================
        // 1. CREATE FUNCTION (Mark Present)
        // ==========================================
        public bool Create(SqlConnection conn, Attendance attendance, out string message)
        {
            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageAttendance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "INSERT");
                    cmd.Parameters.AddWithValue("@LectureEvents_Id", attendance.LectureEvents_Id);
                    cmd.Parameters.AddWithValue("@Student_Id", attendance.Student_Id);

                    SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    cmd.ExecuteNonQuery();

                    message = messageParam.Value?.ToString() ?? string.Empty;
                    return Convert.ToInt32(statusParam.Value) == 1;
                }
            }
            catch (SqlException)
            {
                message = "We are currently having trouble connecting to the database. Please check your connection or try again later.";
                return false;
            }
            catch (Exception)
            {
                message = "An unexpected application error occurred. Please restart the application and try again.";
                return false;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        // ==========================================
        // 2. CLEAR DAY FUNCTION (Bulk Reset)
        // ==========================================
        public bool ClearDay(SqlConnection conn, int lectureEventsId, out string message)
        {
            message = "Cleared successfully";
            try
            {
                string query = "DELETE FROM Attendance WHERE LectureEvents_Id = @LectureEvents_Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LectureEvents_Id", lectureEventsId);

                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        // ==========================================
        // 3. DELETE FUNCTION (Remove single record)
        // ==========================================
        public bool Delete(SqlConnection conn, int id, out string message)
        {
            if (id <= 0)
            {
                message = "Validation Failed: You must provide a valid Attendance ID to delete.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageAttendance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@Id", id);

                    SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    cmd.ExecuteNonQuery();

                    message = messageParam.Value?.ToString() ?? string.Empty;
                    return Convert.ToInt32(statusParam.Value) == 1;
                }
            }
            catch (SqlException)
            {
                message = "We are currently having trouble connecting to the database. Please try again later.";
                return false;
            }
            catch (Exception)
            {
                message = "An unexpected application error occurred. Please restart the application.";
                return false;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        // ==========================================
        // 4. SELECT DETAILS FUNCTION (Read from View)
        // ==========================================
        public List<AttendanceDetailsView> SelectDetails(SqlConnection conn, AttendanceDetailsView searchCriteria, out string message, out int status)
        {
            message = "Data retrieved successfully.";
            status = 1;
            List<AttendanceDetailsView> attendanceRecords = new List<AttendanceDetailsView>();

            try
            {
                string query = "";

                int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
                int searchSchedule = (searchCriteria != null) ? searchCriteria.Course_Schedules_Id : 0;
                int searchStudent = (searchCriteria != null) ? searchCriteria.Student_Id : 0;
                DateTime searchDate = (searchCriteria != null) ? searchCriteria.AttendanceDate : DateTime.MinValue;

                if (searchId > 0)
                {
                    query = "SELECT * FROM vw_AttendanceDetails WHERE Id = @SearchId";
                }
                else if (searchSchedule > 0 && searchDate != DateTime.MinValue)
                {
                    query = "SELECT * FROM vw_AttendanceDetails WHERE Course_Schedules_Id = @SearchSchedule AND CAST(AttendanceDate AS DATE) = CAST(@SearchDate AS DATE)";
                }
                else if (searchSchedule > 0)
                {
                    query = "SELECT * FROM vw_AttendanceDetails WHERE Course_Schedules_Id = @SearchSchedule";
                }
                else if (searchStudent > 0)
                {
                    query = "SELECT * FROM vw_AttendanceDetails WHERE Student_Id = @SearchStudent";
                }
                else
                {
                    query = "SELECT * FROM vw_AttendanceDetails";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchSchedule", searchSchedule > 0 ? (object)searchSchedule : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchStudent", searchStudent > 0 ? (object)searchStudent : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchDate", searchDate != DateTime.MinValue ? (object)searchDate : DBNull.Value);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AttendanceDetailsView record = new AttendanceDetailsView
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Course_Schedules_Id = reader.GetInt32(reader.GetOrdinal("Course_Schedules_Id")),
                                AttendanceDate = reader.GetDateTime(reader.GetOrdinal("AttendanceDate")),
                                Student_Id = reader.GetInt32(reader.GetOrdinal("Student_Id")),

                                StudentName = reader.IsDBNull(reader.GetOrdinal("StudentName"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("StudentName")),

                                CourseName = reader.IsDBNull(reader.GetOrdinal("CourseName"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("CourseName")),

                                CourseCode = reader.IsDBNull(reader.GetOrdinal("CourseCode"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("CourseCode")),

                                ScheduleDay = reader.IsDBNull(reader.GetOrdinal("ScheduleDay"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("ScheduleDay")),

                                StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime"))
                                    ? TimeSpan.Zero : reader.GetTimeSpan(reader.GetOrdinal("StartTime")),

                                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime"))
                                    ? TimeSpan.Zero : reader.GetTimeSpan(reader.GetOrdinal("EndTime"))
                            };

                            attendanceRecords.Add(record);
                        }
                    }
                }

                return attendanceRecords;
            }
            catch (SqlException)
            {
                message = "We are currently having trouble connecting to the database.";
                status = 0;
                return new List<AttendanceDetailsView>();
            }
            catch (Exception)
            {
                message = "An unexpected application error occurred. Please restart the application and try again.";
                status = 0;
                return new List<AttendanceDetailsView>();
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        // ==========================================
        // 5. LECTURE DATES FUNCTION (Cleaned to pull cs.Type directly)
        // ==========================================
        // ==========================================
        // 5. LECTURE DATES FUNCTION (Fixed with correct ERD schema joins)
        // ==========================================
        public List<LectureDateView> GetLectureDates(SqlConnection conn, int scheduleId, out int status, out string message)
        {
            var list = new List<LectureDateView>();
            status = 1;
            message = "Success";

            string query = @"
        SELECT 
            le.Id AS LectureEventId, 
            le.Date_Of_Event, 
            cs.Type AS ScheduleType,
            COUNT(a.Student_Id) as StudentsAttended
        FROM Lecture_Events le
        INNER JOIN Course_Schedules cs ON le.Schedule_Id = cs.Id
        LEFT JOIN Attendance a ON le.Id = a.LectureEvents_Id
        WHERE le.Schedule_Id = @ScheduleId
        GROUP BY le.Id, le.Date_Of_Event, cs.Type
        ORDER BY le.Date_Of_Event DESC";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
                try
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new LectureDateView
                            {
                                ScheduleId = reader.GetInt32(reader.GetOrdinal("LectureEventId")),
                                AttendanceDate = reader.GetDateTime(reader.GetOrdinal("Date_Of_Event")),
                                ScheduleType = reader.IsDBNull(reader.GetOrdinal("ScheduleType")) ? "Session" : reader.GetString(reader.GetOrdinal("ScheduleType")),
                                StudentsAttended = reader.GetInt32(reader.GetOrdinal("StudentsAttended"))
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

        // ==========================================
        // 6. GET INSTRUCTOR ATTENDANCE SHEET (All Enrolled Students + Attendance Status)
        // ==========================================
        // ==========================================
        // 6. GET INSTRUCTOR ATTENDANCE SHEET (Robust & Schema-Aligned)
        // ==========================================
        // ==========================================
        // 6. GET INSTRUCTOR ATTENDANCE SHEET (With Content_Covered)
        // ==========================================
        // ==========================================
        // 6. GET INSTRUCTOR ATTENDANCE SHEET
        // ==========================================
        public List<AttendanceDetailsView> GetInstructorAttendanceSheet(SqlConnection conn, int courseId, int lectureEventsId, out string message, out int status)
        {
            message = "Attendance sheet retrieved successfully.";
            status = 1;
            List<AttendanceDetailsView> list = new List<AttendanceDetailsView>();

            try
            {
                string query = @"
                    SELECT 
                         ISNULL(A.Id, 0) AS Id,
                         LE.Id AS LectureEvents_Id,
                         CS.Id AS Course_Schedules_Id,
                         LE.Date_Of_Event AS AttendanceDate,
                         LE.Content_Covered,
                         S.Id AS Student_Id,
                         S.Name AS StudentName,
                         C.Name AS CourseName,
                         C.Code AS CourseCode,
                         CS.Day AS ScheduleDay,
                         CS.Type AS ScheduleType,
                         CS.StartTime,
                         CS.EndTime
                    FROM Lecture_Events LE
                    INNER JOIN Course_Schedules CS ON LE.Schedule_Id = CS.Id
                    INNER JOIN Instructors_Courses IC ON CS.Instructors_Courses_Id = IC.Id
                    INNER JOIN Courses C ON IC.Course_Id = C.Id
                    INNER JOIN Enrollments E ON E.Course_Id = C.Id AND E.Status_Id = 1
                    INNER JOIN Students S ON E.Student_Id = S.Id
                    LEFT JOIN Attendance A ON A.LectureEvents_Id = LE.Id AND A.Student_Id = S.Id
                    WHERE LE.Id = @LectureEventsId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LectureEventsId", lectureEventsId);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AttendanceDetailsView record = new AttendanceDetailsView
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                LectureEvents_Id = reader.GetInt32(reader.GetOrdinal("LectureEvents_Id")),
                                Course_Schedules_Id = reader.GetInt32(reader.GetOrdinal("Course_Schedules_Id")),
                                AttendanceDate = reader.GetDateTime(reader.GetOrdinal("AttendanceDate")),
                                Content_Covered = reader.IsDBNull(reader.GetOrdinal("Content_Covered")) ? string.Empty : reader.GetString(reader.GetOrdinal("Content_Covered")),
                                Student_Id = reader.GetInt32(reader.GetOrdinal("Student_Id")),
                                StudentName = reader.GetString(reader.GetOrdinal("StudentName")),
                                CourseName = reader.GetString(reader.GetOrdinal("CourseName")),
                                CourseCode = reader.GetString(reader.GetOrdinal("CourseCode")),
                                ScheduleDay = reader.GetString(reader.GetOrdinal("ScheduleDay")),
                                ScheduleType = reader.GetString(reader.GetOrdinal("ScheduleType")),
                                StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime"))
                            };

                            list.Add(record);
                        }
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                status = 0;
                message = ex.Message;
                return new List<AttendanceDetailsView>();
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}