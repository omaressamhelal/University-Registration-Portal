using Microsoft.Data.SqlClient;
using Registration.Domain;
using System.Data;

namespace Registration.DataAccess;

public class DepartmentOperations
{
    // ==========================================
    // 1. CREATE FUNCTION
    // ==========================================
    public bool Create(SqlConnection conn, Department department, out string message)
    {
        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageDepartment", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "INSERT");
                cmd.Parameters.AddWithValue("@Name", department.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(department.Name_AR) ? (object)DBNull.Value : department.Name_AR);
                cmd.Parameters.AddWithValue("@Code", department.Code);

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
    public bool Update(SqlConnection conn, Department department, out string message)
    {
        if (department.Id <= 0)
        {
            message = "Validation Failed: You must provide a valid Department ID to update a record.";
            return false;
        }

        message = string.Empty;

        try
        {
            // Fixed: changed from usp_ManageCourse to usp_ManageDepartment
            using (SqlCommand cmd = new SqlCommand("usp_ManageDepartment", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "UPDATE");
                cmd.Parameters.AddWithValue("@Id", department.Id);
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(department.Name) ? (object)DBNull.Value : department.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(department.Name_AR) ? (object)DBNull.Value : department.Name_AR);

                // Fixed: changed from @@Code to @Code
                cmd.Parameters.AddWithValue("@Code", string.IsNullOrEmpty(department.Code) ? (object)DBNull.Value : department.Code);

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
            message = "Validation Failed: You must provide a valid Department ID to delete.";
            return false;
        }

        message = string.Empty;

        try
        {
            // Fixed: changed from usp_ManageCourse to usp_ManageDepartment
            using (SqlCommand cmd = new SqlCommand("usp_ManageDepartment", conn))
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
    public List<DepartmentDetailsView> Select(SqlConnection conn, Department searchCriteria, out string message, out int status)
    {
        message = "Data retrieved successfully.";
        status = 1;
        List<DepartmentDetailsView> departments = new List<DepartmentDetailsView>();

        try
        {
            string query = "";

            int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
            string? searchName = (searchCriteria != null) ? searchCriteria.Name : null;
            string? searchCode = (searchCriteria != null) ? searchCriteria.Code : null;

            if (searchId > 0)
            {
                query = "SELECT * FROM vw_Departments WHERE Id = @SearchId";
            }
            else if (!string.IsNullOrEmpty(searchName))
            {
                query = "SELECT * FROM vw_Departments WHERE Name LIKE '%' + @SearchName + '%'";
            }
            else if (!string.IsNullOrEmpty(searchCode))
            {
                query = "SELECT * FROM vw_Departments WHERE Code LIKE '%' + @SearchCode + '%'";
            }
            else
            {
                query = "SELECT * FROM vw_Departments";
            }

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchName", !string.IsNullOrEmpty(searchName) ? (object)searchName : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchCode", !string.IsNullOrEmpty(searchCode) ? (object)searchCode : DBNull.Value);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DepartmentDetailsView dept = new DepartmentDetailsView();

                        dept.Id = Convert.ToInt32(reader["Id"]);
                        dept.Name = reader["Name"].ToString()!;
                        dept.Code = reader["Code"].ToString()!;
                        dept.TotalStudents = Convert.ToInt32(reader["TotalStudents"]);
                        dept.TotalInstructors = Convert.ToInt32(reader["TotalInstructors"]);
                        dept.TotalCourses = Convert.ToInt32(reader["TotalCourses"]);

                        if (reader["Name_AR"] != DBNull.Value)
                        {
                            dept.Name_AR = reader["Name_AR"].ToString();
                        }

                        if (reader["AverageGpa"] != DBNull.Value)
                        {
                            dept.AverageGpa = Convert.ToDecimal(reader["AverageGpa"]);
                        }

                        departments.Add(dept);
                    }
                }
            }

            return departments;
        }
        catch (SqlException)
        {
            message = "We are currently having trouble connecting to the database.";
            status = 0;
            return new List<DepartmentDetailsView>();
        }
        catch (Exception)
        {
            message = "An unexpected application error occurred. Please restart the application and try again.";
            status = 0;
            return new List<DepartmentDetailsView>();
        }
        finally
        {
            if (conn != null) conn.Close();
        }
    }
}