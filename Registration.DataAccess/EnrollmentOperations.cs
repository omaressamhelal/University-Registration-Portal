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
    public class EnrollmentOperations
    {
        // ==========================================
        // 1. CREATE FUNCTION (Auto-assigns active or newest semester)
        // ==========================================
        public bool Create(SqlConnection conn, Enrollment enrollment, out string message)
        {
            message = string.Empty;

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                // 1. Automatically find the active/open semester ID, with fallback to the newest semester
                int activeSemesterId = 0;
                string semQuery = "SELECT TOP 1 Id FROM Semesters WHERE Is_Registration_Open = 1 ORDER BY Start_date DESC";
                using (SqlCommand semCmd = new SqlCommand(semQuery, conn))
                {
                    var semResult = semCmd.ExecuteScalar();
                    if (semResult != null && semResult != DBNull.Value)
                    {
                        activeSemesterId = Convert.ToInt32(semResult);
                    }
                    else
                    {
                        // Fallback to the latest/newest semester if none have registration explicitly open
                        semCmd.CommandText = "SELECT TOP 1 Id FROM Semesters ORDER BY Start_date DESC";
                        var fallbackResult = semCmd.ExecuteScalar();
                        if (fallbackResult != null && fallbackResult != DBNull.Value)
                        {
                            activeSemesterId = Convert.ToInt32(fallbackResult);
                        }
                    }
                }

                if (activeSemesterId <= 0)
                {
                    message = "Error: No valid semester found in the system to attach this enrollment to.";
                    return false;
                }

                // 2. Execute the enrollment creation via stored procedure
                using (SqlCommand cmd = new SqlCommand("usp_ManageEnrollment", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "INSERT");
                    cmd.Parameters.AddWithValue("@Student_Id", enrollment.StudentId.HasValue ? (object)enrollment.StudentId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Course_Id", enrollment.CourseId.HasValue ? (object)enrollment.CourseId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Semester_Id", activeSemesterId); // <-- Automatically assigned active/newest semester!
                    cmd.Parameters.AddWithValue("@Status_Id", enrollment.StatusId.HasValue ? (object)(int)enrollment.StatusId.Value : DBNull.Value);

                    SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    cmd.ExecuteNonQuery();

                    message = messageParam.Value?.ToString() ?? string.Empty;
                    return Convert.ToInt32(statusParam.Value) == 1;
                }
            }
            catch (SqlException ex)
            {
                message = "Database Error: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                message = "Unexpected Error: " + ex.Message;
                return false;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        // ==========================================
        // 2. UPDATE FUNCTION
        // ==========================================
        public bool Update(SqlConnection conn, Enrollment enrollment, out string message)
        {
            if (enrollment.Id <= 0)
            {
                message = "Validation Failed: You must provide a valid Enrollment ID to update a record.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageEnrollment", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "UPDATE");
                    cmd.Parameters.AddWithValue("@Id", enrollment.Id);
                    cmd.Parameters.AddWithValue("@Student_Id", enrollment.StudentId.HasValue ? (object)enrollment.StudentId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Course_Id", enrollment.CourseId.HasValue ? (object)enrollment.CourseId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Semester_Id", enrollment.SemesterId.HasValue ? (object)enrollment.SemesterId.Value : DBNull.Value);

                    object statusVal = enrollment.StatusId.HasValue ? (object)(int)enrollment.StatusId.Value : DBNull.Value;
                    cmd.Parameters.AddWithValue("@Status_Id", statusVal);

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
            catch (SqlException ex)
            {
                message = "Database Error: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                message = "Unexpected Error: " + ex.Message;
                return false;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        // ==========================================
        // 3. DELETE FUNCTION
        // ==========================================
        public bool Delete(SqlConnection conn, int id, out string message)
        {
            if (id <= 0)
            {
                message = "Validation Failed: You must provide a valid Enrollment ID to delete.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageEnrollment", conn))
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
            catch (SqlException ex)
            {
                message = "Database Error: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                message = "Unexpected Error: " + ex.Message;
                return false;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        // ==========================================
        // 4. SELECT FUNCTION
        // ==========================================
        public List<EnrollmentDetailsView> Select(SqlConnection conn, Enrollment searchCriteria, out string message, out int status)
        {
            message = "Data retrieved successfully.";
            status = 1;
            List<EnrollmentDetailsView> enrollments = new List<EnrollmentDetailsView>();

            try
            {
                string query = "";
                int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
                int? searchStudent = (searchCriteria != null) ? searchCriteria.StudentId : null;
                int? searchCourse = (searchCriteria != null) ? searchCriteria.CourseId : null;
                int? searchStatus = (searchCriteria != null && searchCriteria.StatusId.HasValue) ? (int?)searchCriteria.StatusId.Value : null;

                if (searchId > 0)
                {
                    query = "SELECT * FROM vw_EnrollmentDetails WHERE Id = @SearchId";
                }
                else if (searchStudent.HasValue && searchStudent.Value > 0)
                {
                    query = "SELECT * FROM vw_EnrollmentDetails WHERE StudentId = @SearchStudent";
                }
                else if (searchCourse.HasValue && searchCourse.Value > 0)
                {
                    query = "SELECT * FROM vw_EnrollmentDetails WHERE CourseId = @SearchCourse";
                }
                else if (searchStatus.HasValue)
                {
                    query = "SELECT * FROM vw_EnrollmentDetails WHERE StatusId = @SearchStatus";
                }
                else
                {
                    query = "SELECT * FROM vw_EnrollmentDetails";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchStudent", (searchStudent.HasValue && searchStudent.Value > 0) ? (object)searchStudent.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchCourse", (searchCourse.HasValue && searchCourse.Value > 0) ? (object)searchCourse.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchStatus", searchStatus.HasValue ? (object)searchStatus.Value : DBNull.Value);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EnrollmentDetailsView enrollment = new EnrollmentDetailsView
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                StudentName = reader.IsDBNull(reader.GetOrdinal("StudentName")) ? string.Empty : reader.GetString(reader.GetOrdinal("StudentName")),
                                CourseName = reader.IsDBNull(reader.GetOrdinal("CourseName")) ? string.Empty : reader.GetString(reader.GetOrdinal("CourseName")),
                                SemesterId = reader.IsDBNull(reader.GetOrdinal("SemesterId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("SemesterId")),
                                SemesterName = reader.IsDBNull(reader.GetOrdinal("SemesterName")) ? string.Empty : reader.GetString(reader.GetOrdinal("SemesterName")),
                                StatusName = reader.IsDBNull(reader.GetOrdinal("StatusName")) ? string.Empty : reader.GetString(reader.GetOrdinal("StatusName"))
                            };

                            enrollments.Add(enrollment);
                        }
                    }
                }

                return enrollments;
            }
            catch (SqlException ex)
            {
                message = "Database Error: " + ex.Message;
                status = 0;
                return new List<EnrollmentDetailsView>();
            }
            catch (Exception ex)
            {
                message = "Unexpected Error: " + ex.Message;
                status = 0;
                return new List<EnrollmentDetailsView>();
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}