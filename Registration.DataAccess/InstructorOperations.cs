using Microsoft.Data.SqlClient;
using Registration.Domain;
using Registration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registration.DataAccess;

public class InstructorOperations
{
    // ==========================================
    // 1. CREATE FUNCTION
    // ==========================================
    public bool Create(SqlConnection conn, Instructor instructor, out string message)
    {
        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageInstructor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "INSERT");
                cmd.Parameters.AddWithValue("@Name", instructor.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(instructor.Name_AR) ? (object)DBNull.Value : instructor.Name_AR);
                cmd.Parameters.AddWithValue("@Email", instructor.Email);
                cmd.Parameters.AddWithValue("@Password", instructor.Password);
                cmd.Parameters.AddWithValue("@Start_year", instructor.Start_year);
                cmd.Parameters.AddWithValue("@Department_Id", instructor.Department_Id == null ? (object)DBNull.Value : instructor.Department_Id);
                cmd.Parameters.AddWithValue("@National_Id", instructor.National_Id);
                cmd.Parameters.AddWithValue("@Status_Id", instructor.Status_Id == null ? (object)DBNull.Value : instructor.Status_Id);
                cmd.Parameters.AddWithValue("@Salary", instructor.Salary == null ? (object)DBNull.Value : instructor.Salary);

                // 🌟 ADDED: Pass Office Hours on Create
                cmd.Parameters.AddWithValue("@OfficeHours", string.IsNullOrEmpty(instructor.OfficeHours) ? (object)DBNull.Value : instructor.OfficeHours);

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
    public bool Update(SqlConnection conn, Instructor instructor, out string message)
    {
        if (instructor.Id <= 0)
        {
            message = "Validation Failed: You must provide a valid Instructor ID to update a record.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageInstructor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "UPDATE");
                cmd.Parameters.AddWithValue("@Id", instructor.Id);

                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(instructor.Name) ? (object)DBNull.Value : instructor.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(instructor.Name_AR) ? (object)DBNull.Value : instructor.Name_AR);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(instructor.Email) ? (object)DBNull.Value : instructor.Email);
                cmd.Parameters.AddWithValue("@Password", string.IsNullOrEmpty(instructor.Password) ? (object)DBNull.Value : instructor.Password);
                cmd.Parameters.AddWithValue("@National_Id", string.IsNullOrEmpty(instructor.National_Id) ? (object)DBNull.Value : instructor.National_Id);

                cmd.Parameters.AddWithValue("@Salary", instructor.Salary == null ? (object)DBNull.Value : instructor.Salary);
                cmd.Parameters.AddWithValue("@Start_year", instructor.Start_year == null ? (object)DBNull.Value : instructor.Start_year);
                cmd.Parameters.AddWithValue("@Department_Id", instructor.Department_Id == null ? (object)DBNull.Value : instructor.Department_Id);
                cmd.Parameters.AddWithValue("@Status_Id", instructor.Status_Id == null ? (object)DBNull.Value : instructor.Status_Id);

                // 🌟 ADDED: Pass Office Hours on Update
                cmd.Parameters.AddWithValue("@OfficeHours", string.IsNullOrEmpty(instructor.OfficeHours) ? (object)DBNull.Value : instructor.OfficeHours);

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
            message = "Validation Failed: You must provide a valid ID to delete.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageInstructor", conn))
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
    public List<InstructorDetailsView> Select(SqlConnection conn, Instructor searchCriteria, out string message, out int status)
    {
        message = "Data retrieved successfully.";
        status = 1;
        List<InstructorDetailsView> instructors = new List<InstructorDetailsView>();

        try
        {
            string query = "";

            int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
            string searchEmail = (searchCriteria != null) ? searchCriteria.Email : null;

            if (searchId > 0)
            {
                query = "SELECT * FROM vw_Instructors WHERE Id = @SearchId";
            }
            else if (!string.IsNullOrEmpty(searchEmail))
            {
                query = "SELECT * FROM vw_Instructors WHERE Email = @SearchEmail";
            }
            else
            {
                query = "SELECT * FROM vw_Instructors";
            }

            if (conn.State != ConnectionState.Open)
                conn.Open();

            // 🌟 1. Fetch the Active Semester dynamically based on today's date
            string activeSemesterName = "Fall 2026"; // Fallback default
            string semQuery = "SELECT TOP 1 Name FROM Semesters WHERE GETDATE() BETWEEN Start_date AND End_date ORDER BY Start_date DESC";
            using (SqlCommand semCmd = new SqlCommand(semQuery, conn))
            {
                var semResult = semCmd.ExecuteScalar();
                if (semResult != null)
                {
                    activeSemesterName = semResult.ToString()!;
                }
            }

            // 2. Fetch Instructor Details
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchEmail", !string.IsNullOrEmpty(searchEmail) ? (object)searchEmail : DBNull.Value);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        InstructorDetailsView i = new InstructorDetailsView();

                        i.Id = Convert.ToInt32(reader["Id"]);
                        i.InstructorName = reader["InstructorName"].ToString()!;
                        i.Email = reader["Email"].ToString()!;
                        i.Password = reader["Password"].ToString()!;
                        i.National_Id = reader["National_Id"].ToString()!;
                        i.Start_year = Convert.ToInt32(reader["Start_year"]);
                        i.Salary = Convert.ToSingle(reader["Salary"]);

                        i.DepartmentName = reader["DepartmentName"].ToString()!;
                        i.StatusDescription = reader["StatusDescription"].ToString()!;

                        i.TaughtCourses = reader["TaughtCourses"] != DBNull.Value ? reader["TaughtCourses"].ToString()! : "No courses assigned";
                        i.CourseCodes = reader["CourseCodes"] != DBNull.Value ? reader["CourseCodes"].ToString()! : "";
                        i.CourseIds = reader["CourseIds"] != DBNull.Value ? reader["CourseIds"].ToString()! : "";
                        i.ScheduleDays = reader["ScheduleDays"] != DBNull.Value ? reader["ScheduleDays"].ToString()! : "";
                        i.ScheduleTimes = reader["ScheduleTimes"] != DBNull.Value ? reader["ScheduleTimes"].ToString()! : "";
                        i.ScheduleTypes = reader["ScheduleTypes"] != DBNull.Value ? reader["ScheduleTypes"].ToString()! : "";
                        i.OfficeHours = reader["OfficeHours"] != DBNull.Value ? reader["OfficeHours"].ToString()! : "Not specified yet";

                        // 🌟 Assign the dynamic active semester name
                        i.ActiveSemesterName = activeSemesterName;

                        if (reader["InstructorName_AR"] != DBNull.Value)
                        {
                            i.InstructorName_AR = reader["InstructorName_AR"].ToString()!;
                        }

                        instructors.Add(i);
                    }
                }
            }

            return instructors;
        }
        catch (SqlException)
        {
            message = "We are currently having trouble connecting to the database.";
            status = 0;
            return new List<InstructorDetailsView>();
        }
        catch (Exception)
        {
            message = "An unexpected application error occurred. Please restart the application and try again.";
            status = 0;
            return new List<InstructorDetailsView>();
        }
        finally
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Open)
                conn.Close();
        }
    }

    public bool UpdateStatusOnly(SqlConnection conn, int id, string statusDescription, out string message)
    {
        try
        {
            if (statusDescription == "OnLeave")
            {
                statusDescription = "On Leave";
            }

            string query = @"UPDATE Instructors 
                             SET Status_Id = (SELECT Id FROM InstructorsStatus WHERE Description = @Status OR Name = @Status) 
                             WHERE Id = @Id";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Status", statusDescription);
                cmd.Parameters.AddWithValue("@Id", id);

                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    message = "Instructor status updated successfully.";
                    return true;
                }
                message = "Instructor not found with ID " + id;
                return false;
            }
        }
        catch (SqlException)
        {
            message = "We are currently having trouble connecting to the database.";
            return false;
        }
        catch (Exception)
        {
            message = "An unexpected application error occurred. Please restart the application and try again.";
            return false;
        }
        finally
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Open)
                conn.Close();
        }
    }
    public bool UpdateContactInfo(SqlConnection conn, int instructorId, string email, string officeHours, out string message)
    {
        message = string.Empty;
        try
        {
            // 1. Write the precise UPDATE query
            string query = @"
            UPDATE Instructors 
            SET Email = @Email, 
                OfficeHours = @OfficeHours 
            WHERE Id = @Id";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // 2. Safely bind the parameters to prevent SQL injection
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OfficeHours", string.IsNullOrWhiteSpace(officeHours) ? (object)DBNull.Value : officeHours);
                cmd.Parameters.AddWithValue("@Id", instructorId);

                // 3. Open connection and execute
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    message = "Profile updated successfully!";
                    return true;
                }
                else
                {
                    message = "Failed to find the instructor record to update.";
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            message = $"Database error: {ex.Message}";
            return false;
        }
        finally
        {
            if (conn != null) conn.Close();
        }
    }
}