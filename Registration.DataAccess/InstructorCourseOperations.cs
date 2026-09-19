using Microsoft.Data.SqlClient;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registration.DataAccess_
{
    public class InstructorCourseOperations
    {
        // ==========================================
        // 1. CREATE FUNCTION
        // ==========================================
        public bool Create(SqlConnection conn, InstructorCourse assignment, out string message)
        {
            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageInstructorCourse", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Action is INSERT, no ID needed!
                    cmd.Parameters.AddWithValue("@Action", "INSERT");

                    // 2. The Instructor Course Properties
                    cmd.Parameters.AddWithValue("@Instructor_id", assignment.InstructorId);
                    cmd.Parameters.AddWithValue("@Course_id", assignment.CourseId);
                    cmd.Parameters.AddWithValue("@Semester_Id", assignment.Semester_Id); // 🌟 Added Semester_Id

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
        public bool Update(SqlConnection conn, InstructorCourse assignment, out string message)
        {
            if (assignment.Id <= 0)
            {
                message = "Validation Failed: You must provide a valid Assignment ID to update a record.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageInstructorCourse", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Set the action to UPDATE
                    cmd.Parameters.AddWithValue("@Action", "UPDATE");

                    // 2. CRITICAL: Pass the ID so SQL Server knows which row to update
                    cmd.Parameters.AddWithValue("@Id", assignment.Id);

                    // 3. parameters
                    cmd.Parameters.AddWithValue("@Instructor_id", assignment.InstructorId == 0 ? (object)DBNull.Value : assignment.InstructorId);
                    cmd.Parameters.AddWithValue("@Course_id", assignment.CourseId == 0 ? (object)DBNull.Value : assignment.CourseId);
                    cmd.Parameters.AddWithValue("@Semester_Id", assignment.Semester_Id == 0 ? (object)DBNull.Value : assignment.Semester_Id); // 🌟 Added Semester_Id

                    // 5. Output Parameters
                    SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    // 6. Open connection and Execute
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    cmd.ExecuteNonQuery();

                    // 7. Unpack results
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
                message = "Validation Failed: You must provide a valid Assignment ID to delete.";
                return false;
            }

            message = string.Empty;

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                // 🌟 STEP 1: CASCADING DELETE PRE-CLEANUP
                string cleanupQuery = @"
                    DELETE a
                    FROM Attendance a
                    INNER JOIN Lecture_Events le ON a.LectureEvents_Id = le.Id
                    INNER JOIN Course_Schedules cs ON le.Schedule_Id = cs.Id
                    WHERE cs.Instructors_Courses_Id = @Id;

                    DELETE le
                    FROM Lecture_Events le
                    INNER JOIN Course_Schedules cs ON le.Schedule_Id = cs.Id
                    WHERE cs.Instructors_Courses_Id = @Id;

                    DELETE FROM Course_Schedules
                    WHERE Instructors_Courses_Id = @Id;
                ";

                using (SqlCommand cleanupCmd = new SqlCommand(cleanupQuery, conn))
                {
                    cleanupCmd.Parameters.AddWithValue("@Id", id);
                    cleanupCmd.ExecuteNonQuery();
                }

                // 🌟 STEP 2: DELETE THE MAIN ASSIGNMENT
                using (SqlCommand cmd = new SqlCommand("usp_ManageInstructorCourse", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@Id", id);

                    SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

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
        // 4. SELECT FUNCTION
        // ==========================================
        public List<InstructorCourseDetailsView> Select(SqlConnection conn, InstructorCourse searchCriteria, out string message, out int status)
        {
            message = "Data retrieved successfully.";
            status = 1;
            List<InstructorCourseDetailsView> assignments = new List<InstructorCourseDetailsView>();

            try
            {
                string query = "";
                int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
                int? searchInstructor = (searchCriteria != null && searchCriteria.InstructorId > 0) ? searchCriteria.InstructorId : (int?)null;
                int? searchCourse = (searchCriteria != null && searchCriteria.CourseId > 0) ? searchCriteria.CourseId : (int?)null;

                if (searchId > 0)
                {
                    query = "SELECT * FROM vw_InstructorCourses WHERE Id = @SearchId";
                }
                else if (searchInstructor.HasValue)
                {
                    query = "SELECT * FROM vw_InstructorCourses WHERE Instructor_id = @SearchInstructor";
                }
                else if (searchCourse.HasValue)
                {
                    query = "SELECT * FROM vw_InstructorCourses WHERE Course_id = @SearchCourse";
                }
                else
                {
                    query = "SELECT * FROM vw_InstructorCourses";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchInstructor", searchInstructor.HasValue ? (object)searchInstructor.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchCourse", searchCourse.HasValue ? (object)searchCourse.Value : DBNull.Value);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InstructorCourseDetailsView item = new InstructorCourseDetailsView
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                InstructorName = reader.IsDBNull(reader.GetOrdinal("InstructorName"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("InstructorName")),
                                CourseName = reader.IsDBNull(reader.GetOrdinal("CourseName"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("CourseName")),
                                Semester_Id = reader.IsDBNull(reader.GetOrdinal("Semester_Id"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("Semester_Id")) // 🌟 Read Semester_Id from view
                            };

                            assignments.Add(item);
                        }
                    }
                }

                return assignments;
            }
            catch (SqlException)
            {
                message = "We are currently having trouble connecting to the database.";
                status = 0;
                return new List<InstructorCourseDetailsView>();
            }
            catch (Exception)
            {
                message = "An unexpected application error occurred. Please restart the application and try again.";
                status = 0;
                return new List<InstructorCourseDetailsView>();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
    }
}