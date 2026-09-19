using Microsoft.Data.SqlClient;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace Registration.DataAccess
{
    public class LectureEventOperations
    {
        // ==========================================
        // 1. CREATE FUNCTION
        // ==========================================
        public bool Create(SqlConnection conn, LectureEvent lectureEvent, out string message)
        {
            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageLectureEvent", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Action is INSERT, no ID needed!
                    cmd.Parameters.AddWithValue("@Action", "INSERT");

                    // 2. The Lecture Event Properties
                    cmd.Parameters.AddWithValue("@Schedule_Id", lectureEvent.ScheduleId);
                    cmd.Parameters.AddWithValue("@Date_Of_Event", lectureEvent.DateOfEvent);
                    cmd.Parameters.AddWithValue("@Content_Covered", string.IsNullOrWhiteSpace(lectureEvent.ContentCovered) ? (object)DBNull.Value : lectureEvent.ContentCovered);

                    // 3. Output Parameters for Status and Message
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
                if (conn != null) conn.Close();
            }
        }

        // ==========================================
        // 2. UPDATE FUNCTION
        // ==========================================
        public bool Update(SqlConnection conn, LectureEvent lectureEvent, out string message)
        {
            if (lectureEvent.Id <= 0)
            {
                message = "Validation Failed: You must provide a valid Lecture Event ID to update a record.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageLectureEvent", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Set the action to UPDATE
                    cmd.Parameters.AddWithValue("@Action", "UPDATE");

                    // 2. CRITICAL: Pass the ID so SQL Server knows which row to update
                    cmd.Parameters.AddWithValue("@Id", lectureEvent.Id);

                    // 3. Parameters
                    cmd.Parameters.AddWithValue("@Schedule_Id", lectureEvent.ScheduleId);
                    cmd.Parameters.AddWithValue("@Date_Of_Event", lectureEvent.DateOfEvent);
                    cmd.Parameters.AddWithValue("@Content_Covered", string.IsNullOrWhiteSpace(lectureEvent.ContentCovered) ? (object)DBNull.Value : lectureEvent.ContentCovered);

                    // 4. Output Parameters
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
                if (conn != null) conn.Close();
            }
        }

        // ==========================================
        // 3. DELETE FUNCTION
        // ==========================================
        public bool Delete(SqlConnection conn, int id, out string message)
        {
            if (id <= 0)
            {
                message = "Validation Failed: You must provide a valid Lecture Event ID to delete.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageLectureEvent", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Action is DELETE
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@Id", id);

                    // 2. Output Parameters
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
                if (conn != null) conn.Close();
            }
        }

        // ==========================================
        // 4. SELECT FUNCTION (For the Modal Dropdown)
        // ==========================================
        public List<LectureDateView> SelectByCourse(SqlConnection conn, int courseId, out string message, out int status)
        {
            message = "Data retrieved successfully.";
            status = 1;
            List<LectureDateView> lectures = new List<LectureDateView>();

            if (courseId <= 0)
            {
                message = "Invalid Course ID provided.";
                status = 0;
                return lectures;
            }

            try
            {
                // 🌟 UPDATED QUERY: Selects CS.Type (or CS.Schedule_Type depending on your exact column name) 
                // and maps it to ScheduleType so the past sessions dropdown shows Lecture, Lab, or Section correctly.
                string query = @"
                    SELECT 
                        LE.Id AS ScheduleId, 
                        LE.Date_Of_Event AS AttendanceDate, 
                        CS.Type AS ScheduleType,
                        COUNT(A.Id) AS StudentsAttended
                    FROM Lecture_Events LE
                    INNER JOIN Course_Schedules CS ON LE.Schedule_Id = CS.Id
                    INNER JOIN Instructors_Courses IC ON CS.Instructors_Courses_Id = IC.Id
                    LEFT JOIN Attendance A ON A.LectureEvents_Id = LE.Id
                    WHERE IC.Course_Id = @CourseId
                    GROUP BY LE.Id, LE.Date_Of_Event, CS.Type
                    ORDER BY LE.Date_Of_Event DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CourseId", courseId);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LectureDateView lecture = new LectureDateView
                            {
                                ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                                AttendanceDate = reader.GetDateTime(reader.GetOrdinal("AttendanceDate")),
                                ScheduleType = reader["ScheduleType"] != DBNull.Value ? reader["ScheduleType"].ToString() : "Lecture",
                                StudentsAttended = reader.GetInt32(reader.GetOrdinal("StudentsAttended"))
                            };

                            lectures.Add(lecture);
                        }
                    }
                }

                return lectures;
            }
            catch (SqlException)
            {
                message = "We are currently having trouble connecting to the database.";
                status = 0;
                return new List<LectureDateView>();
            }
            catch (Exception)
            {
                message = "An unexpected application error occurred. Please restart the application and try again.";
                status = 0;
                return new List<LectureDateView>();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
    }
}