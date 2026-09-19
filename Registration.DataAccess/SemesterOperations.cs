using Microsoft.Data.SqlClient;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace Registration.DataAccess;

public class SemesterOperations
{
    // ==========================================
    // 1. CREATE FUNCTION
    // ==========================================
    public bool Create(SqlConnection conn, Semester semester, out string message)
    {
        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageSemester", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // 1. Action is INSERT, no ID needed!
                cmd.Parameters.AddWithValue("@Action", "INSERT");

                // 2. The Semester Properties
                cmd.Parameters.AddWithValue("@Name", semester.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(semester.Name_AR) ? (object)DBNull.Value : semester.Name_AR);
                cmd.Parameters.AddWithValue("@Start_date", semester.Start_date.HasValue ? (object)semester.Start_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@End_date", semester.End_date.HasValue ? (object)semester.End_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Is_Registration_Open", semester.Is_Registration_Open.HasValue ? (object)semester.Is_Registration_Open.Value : DBNull.Value);

                // 3. Output Parameters
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
            if (conn != null && conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    // ==========================================
    // 2. UPDATE FUNCTION
    // ==========================================
    public bool Update(SqlConnection conn, Semester semester, out string message)
    {
        if (semester.Id <= 0)
        {
            message = "Validation Failed: You must provide a valid Semester ID to update a record.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageSemester", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // 1. Set the action to UPDATE
                cmd.Parameters.AddWithValue("@Action", "UPDATE");

                // 2. Pass the ID
                cmd.Parameters.AddWithValue("@Id", semester.Id);

                // 3. Parameters with COALESCE/DBNull handling
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(semester.Name) ? (object)DBNull.Value : semester.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(semester.Name_AR) ? (object)DBNull.Value : semester.Name_AR);
                cmd.Parameters.AddWithValue("@Start_date", semester.Start_date.HasValue ? (object)semester.Start_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@End_date", semester.End_date.HasValue ? (object)semester.End_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Is_Registration_Open", semester.Is_Registration_Open.HasValue ? (object)semester.Is_Registration_Open.Value : DBNull.Value);

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
            message = "Validation Failed: You must provide a valid Semester ID to delete.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageSemester", conn))
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
            if (conn != null && conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    // ==========================================
    // 4. SELECT FUNCTION
    // ==========================================
    public List<Semester> Select(SqlConnection conn, Semester? searchCriteria, out string message, out int status)
    {
        message = "Data retrieved successfully.";
        status = 1;
        List<Semester> semesters = new List<Semester>();

        try
        {
            string query = "";

            int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
            string? searchName = (searchCriteria != null) ? searchCriteria.Name : null;

            if (searchId > 0)
            {
                query = "SELECT * FROM Semesters WHERE Id = @SearchId";
            }
            else if (!string.IsNullOrEmpty(searchName))
            {
                // FIX: Added ORDER BY Start_date DESC here as well
                query = "SELECT * FROM Semesters WHERE Name LIKE '%' + @SearchName + '%' ORDER BY Start_date DESC";
            }
            else
            {
                // FIX: Ensured fallback orders newest first
                query = "SELECT * FROM Semesters ORDER BY Start_date DESC";
            }

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchName", !string.IsNullOrEmpty(searchName) ? (object)searchName : DBNull.Value);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Semester semester = new Semester();

                        semester.Id = Convert.ToInt32(reader["Id"]);
                        semester.Name = reader["Name"].ToString()!;

                        if (reader["Name_AR"] != DBNull.Value)
                        {
                            semester.Name_AR = reader["Name_AR"].ToString();
                        }

                        if (reader["Start_date"] != DBNull.Value)
                        {
                            semester.Start_date = Convert.ToDateTime(reader["Start_date"]);
                        }

                        if (reader["End_date"] != DBNull.Value)
                        {
                            semester.End_date = Convert.ToDateTime(reader["End_date"]);
                        }

                        if (reader["Is_Registration_Open"] != DBNull.Value)
                        {
                            semester.Is_Registration_Open = Convert.ToBoolean(reader["Is_Registration_Open"]);
                        }

                        semesters.Add(semester);
                    }
                }
            }

            return semesters;
        }
        catch (SqlException)
        {
            message = "We are currently having trouble connecting to the database.";
            status = 0;
            return new List<Semester>();
        }
        catch (Exception)
        {
            message = "An unexpected application error occurred. Please restart the application and try again.";
            status = 0;
            return new List<Semester>();
        }
        finally
        {
            if (conn != null && conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    // ==========================================
    // 5. OPEN REGISTRATION (Quick Toggle)
    // ==========================================
    public bool OpenRegistration(SqlConnection conn, int id, out string message)
    {
        message = string.Empty;
        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageSemester", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "OPEN_REG");
                cmd.Parameters.AddWithValue("@Id", id);

                SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(statusParam);
                cmd.Parameters.Add(messageParam);

                if (conn.State != ConnectionState.Open) conn.Open();
                cmd.ExecuteNonQuery();

                message = messageParam.Value?.ToString() ?? string.Empty;
                return Convert.ToInt32(statusParam.Value) == 1;
            }
        }
        catch (Exception ex)
        {
            message = "Error: " + ex.Message;
            return false;
        }
        finally { if (conn != null) conn.Close(); }
    }

    // ==========================================
    // 6. CLOSE REGISTRATION (Quick Toggle)
    // ==========================================
    public bool CloseRegistration(SqlConnection conn, int id, out string message)
    {
        message = string.Empty;
        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageSemester", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "CLOSE_REG");
                cmd.Parameters.AddWithValue("@Id", id);

                SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(statusParam);
                cmd.Parameters.Add(messageParam);

                if (conn.State != ConnectionState.Open) conn.Open();
                cmd.ExecuteNonQuery();

                message = messageParam.Value?.ToString() ?? string.Empty;
                return Convert.ToInt32(statusParam.Value) == 1;
            }
        }
        catch (Exception ex)
        {
            message = "Error: " + ex.Message;
            return false;
        }
        finally { if (conn != null) conn.Close(); }
    }
}