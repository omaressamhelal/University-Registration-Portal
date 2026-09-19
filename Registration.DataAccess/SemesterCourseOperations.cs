using Microsoft.Data.SqlClient;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace Registration.DataAccess
{
    public class SemesterCourseOperations
    {
        // ==========================================
        // 1. CREATE FUNCTION
        // ==========================================
        public bool Create(SqlConnection conn, SemesterCourseView assignment, out string message)
        {
            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageSemesterCourses", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Action is INSERT
                    cmd.Parameters.AddWithValue("@Action", "INSERT");

                    // 2. Properties
                    cmd.Parameters.AddWithValue("@Semester_Id", assignment.Semester_Id);
                    cmd.Parameters.AddWithValue("@Course_Id", assignment.Course_Id);

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
        public bool Update(SqlConnection conn, SemesterCourseView assignment, out string message)
        {
            if (assignment.Id <= 0)
            {
                message = "Validation Failed: You must provide a valid Assignment ID to update a record.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageSemesterCourses", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "UPDATE");
                    cmd.Parameters.AddWithValue("@Id", assignment.Id);

                    cmd.Parameters.AddWithValue("@Semester_Id", assignment.Semester_Id == 0 ? (object)DBNull.Value : assignment.Semester_Id);
                    cmd.Parameters.AddWithValue("@Course_Id", assignment.Course_Id == 0 ? (object)DBNull.Value : assignment.Course_Id);

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
                message = "Validation Failed: You must provide a valid Assignment ID to delete.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageSemesterCourses", conn))
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
                if (conn != null) conn.Close();
            }
        }

        // ==========================================
        // 4. SELECT FUNCTION
        // ==========================================
        public List<SemesterCourseView> Select(SqlConnection conn, SemesterCourseView searchCriteria, out string message, out int status)
        {
            message = "Data retrieved successfully.";
            status = 1;
            List<SemesterCourseView> assignments = new List<SemesterCourseView>();

            try
            {
                string query = "";
                int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
                int? searchSemester = (searchCriteria != null && searchCriteria.Semester_Id > 0) ? searchCriteria.Semester_Id : (int?)null;
                int? searchCourse = (searchCriteria != null && searchCriteria.Course_Id > 0) ? searchCriteria.Course_Id : (int?)null;

                // Pointing to our View (vw_SemesterCourses)
                if (searchId > 0)
                {
                    query = "SELECT * FROM vw_SemesterCourses WHERE Id = @SearchId";
                }
                else if (searchSemester.HasValue)
                {
                    query = "SELECT * FROM vw_SemesterCourses WHERE Semester_Id = @SearchSemester ORDER BY CourseCode";
                }
                else if (searchCourse.HasValue)
                {
                    query = "SELECT * FROM vw_SemesterCourses WHERE Course_Id = @SearchCourse";
                }
                else
                {
                    query = "SELECT * FROM vw_SemesterCourses ORDER BY SemesterName DESC, CourseCode";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchSemester", searchSemester.HasValue ? (object)searchSemester.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchCourse", searchCourse.HasValue ? (object)searchCourse.Value : DBNull.Value);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SemesterCourseView item = new SemesterCourseView
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Semester_Id = reader.GetInt32(reader.GetOrdinal("Semester_Id")),
                                Course_Id = reader.GetInt32(reader.GetOrdinal("Course_Id")),

                                SemesterName = reader.IsDBNull(reader.GetOrdinal("SemesterName"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("SemesterName")),

                                CourseCode = reader.IsDBNull(reader.GetOrdinal("CourseCode"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("CourseCode")),

                                CourseName = reader.IsDBNull(reader.GetOrdinal("CourseName"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("CourseName")),

                                Credit_Hours = reader.IsDBNull(reader.GetOrdinal("Credit_Hours"))
                                    ? 0 : reader.GetInt32(reader.GetOrdinal("Credit_Hours")),

                                Difficulty = reader.IsDBNull(reader.GetOrdinal("Difficulty"))
                                    ? 0 : reader.GetInt32(reader.GetOrdinal("Difficulty")),

                                // NEW: Read DepartmentCode from the view
                                DepartmentCode = reader.IsDBNull(reader.GetOrdinal("DepartmentCode"))
                                    ? string.Empty : reader.GetString(reader.GetOrdinal("DepartmentCode"))
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
                return new List<SemesterCourseView>();
            }
            catch (Exception)
            {
                message = "An unexpected application error occurred. Please restart the application and try again.";
                status = 0;
                return new List<SemesterCourseView>();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
    }
}