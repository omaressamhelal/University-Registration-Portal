using Microsoft.Data.SqlClient;
using Registration.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace Registration.DataAccess;

public class AdminOperations
{
    // ==========================================
    // 1. CREATE FUNCTION
    // ==========================================
    public bool Create(SqlConnection conn, Admin admin, out string message)
    {
        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageAdmins", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "INSERT");
                cmd.Parameters.AddWithValue("@Name", admin.Name);
                cmd.Parameters.AddWithValue("@Email", admin.Email);
                cmd.Parameters.AddWithValue("@Password", admin.Password);
                cmd.Parameters.AddWithValue("@National_id", string.IsNullOrEmpty(admin.National_Id) ? (object)DBNull.Value : admin.National_Id);
                cmd.Parameters.AddWithValue("@Start_year", admin.Start_year == 0 ? (object)DBNull.Value : admin.Start_year);
                cmd.Parameters.AddWithValue("@Salary", admin.Salary);
                cmd.Parameters.AddWithValue("@Role", admin.Role);
                cmd.Parameters.AddWithValue("@Status_id", admin.Status_Id);

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
    public bool Update(SqlConnection conn, Admin admin, out string message)
    {
        if (admin.Id <= 0)
        {
            message = "Validation Failed: You must provide a valid Admin ID to update a record.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageAdmins", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "UPDATE");
                cmd.Parameters.AddWithValue("@Id", admin.Id);
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(admin.Name) ? (object)DBNull.Value : admin.Name);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(admin.Email) ? (object)DBNull.Value : admin.Email);
                cmd.Parameters.AddWithValue("@Password", string.IsNullOrEmpty(admin.Password) ? (object)DBNull.Value : admin.Password);
                cmd.Parameters.AddWithValue("@National_id", string.IsNullOrEmpty(admin.National_Id) ? (object)DBNull.Value : admin.National_Id);
                cmd.Parameters.AddWithValue("@Start_year", admin.Start_year == 0 ? (object)DBNull.Value : admin.Start_year);
                cmd.Parameters.AddWithValue("@Salary", admin.Salary > 0 ? (object)admin.Salary : DBNull.Value);
                cmd.Parameters.AddWithValue("@Role", string.IsNullOrEmpty(admin.Role) ? (object)DBNull.Value : admin.Role);
                cmd.Parameters.AddWithValue("@Status_id", admin.Status_Id > 0 ? (object)admin.Status_Id : DBNull.Value);

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
            message = "Validation Failed: You must provide a valid Admin ID to delete.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageAdmins", conn))
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
    public List<AdminDetailsView> Select(SqlConnection conn, Admin searchCriteria, out string message, out int status)
    {
        message = "Data retrieved successfully.";
        status = 1;
        List<AdminDetailsView> admins = new List<AdminDetailsView>();

        try
        {
            string query = "";
            int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
            string? searchName = (searchCriteria != null) ? searchCriteria.Name : null;
            string? searchEmail = (searchCriteria != null) ? searchCriteria.Email : null;

            if (searchId > 0)
            {
                query = "SELECT * FROM vw_Admins WHERE Id = @SearchId";
            }
            else if (!string.IsNullOrEmpty(searchName))
            {
                query = "SELECT * FROM vw_Admins WHERE Name LIKE '%' + @SearchName + '%'";
            }
            else if (!string.IsNullOrEmpty(searchEmail))
            {
                query = "SELECT * FROM vw_Admins WHERE Email LIKE '%' + @SearchEmail + '%'";
            }
            else
            {
                query = "SELECT * FROM vw_Admins";
            }

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchName", !string.IsNullOrEmpty(searchName) ? (object)searchName : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchEmail", !string.IsNullOrEmpty(searchEmail) ? (object)searchEmail : DBNull.Value);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AdminDetailsView admin = new AdminDetailsView
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                            National_id = reader.IsDBNull(reader.GetOrdinal("National_id")) ? string.Empty : reader.GetString(reader.GetOrdinal("National_id")),
                            Start_year = reader.IsDBNull(reader.GetOrdinal("Start_year")) ? 0 : reader.GetInt32(reader.GetOrdinal("Start_year")),
                            Salary = reader.IsDBNull(reader.GetOrdinal("Salary")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Salary")),
                            Role = reader.IsDBNull(reader.GetOrdinal("Role")) ? string.Empty : reader.GetString(reader.GetOrdinal("Role")),
                            Status_id = reader.IsDBNull(reader.GetOrdinal("Status_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Status_id")),
                            StatusDescription = reader.IsDBNull(reader.GetOrdinal("StatusDescription")) ? string.Empty : reader.GetString(reader.GetOrdinal("StatusDescription"))
                        };

                        admins.Add(admin);
                    }
                }
            }

            return admins;
        }
        catch (SqlException)
        {
            message = "We are currently having trouble connecting to the database.";
            status = 0;
            return new List<AdminDetailsView>();
        }
        catch (Exception)
        {
            message = "An unexpected application error occurred. Please restart the application and try again.";
            status = 0;
            return new List<AdminDetailsView>();
        }
        finally
        {
            if (conn != null) conn.Close();
        }
    }
}