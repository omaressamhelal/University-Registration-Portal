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

public class CourseOperations
{
    // ==========================================
    // 1. CREATE FUNCTIONS
    // ==========================================
    public bool Create(SqlConnection conn, Course course, out string message)
    {
        return Create(conn, course, 0, out message);
    }

    public bool Create(SqlConnection conn, Course course, int departmentId, out string message)
    {
        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageCourse", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "INSERT");
                cmd.Parameters.AddWithValue("@Name", course.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(course.Name_AR) ? (object)DBNull.Value : course.Name_AR);
                cmd.Parameters.AddWithValue("@Code", course.Code);
                cmd.Parameters.AddWithValue("@Credit_Hours", course.Credit_Hours);
                cmd.Parameters.AddWithValue("@Difficulty", course.Difficulty == null ? (object)DBNull.Value : course.Difficulty);
                cmd.Parameters.AddWithValue("@Status_Id", course.Status_Id);

                SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(statusParam);
                cmd.Parameters.Add(messageParam);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                cmd.ExecuteNonQuery();

                message = messageParam.Value?.ToString() ?? string.Empty;
                bool success = Convert.ToInt32(statusParam.Value) == 1;

                // 🌟 Manage Courses_Departments Junction Table on Insert
                if (success && departmentId > 0)
                {
                    int courseId = 0;
                    using (SqlCommand idCmd = new SqlCommand("SELECT TOP 1 Id FROM Courses WHERE Code = @Code ORDER BY Id DESC", conn))
                    {
                        idCmd.Parameters.AddWithValue("@Code", course.Code);
                        var res = idCmd.ExecuteScalar();
                        if (res != null) courseId = Convert.ToInt32(res);
                    }

                    if (courseId > 0)
                    {
                        using (SqlCommand deptCmd = new SqlCommand(
                            "IF EXISTS (SELECT 1 FROM Courses_Departments WHERE Course_Id = @CourseId) " +
                            "UPDATE Courses_Departments SET Department_Id = @DepartmentId WHERE Course_Id = @CourseId; " +
                            "ELSE " +
                            "INSERT INTO Courses_Departments (Course_Id, Department_Id) VALUES (@CourseId, @DepartmentId);", conn))
                        {
                            deptCmd.Parameters.AddWithValue("@CourseId", courseId);
                            deptCmd.Parameters.AddWithValue("@DepartmentId", departmentId);
                            deptCmd.ExecuteNonQuery();
                        }
                    }
                }

                return success;
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
    // 2. UPDATE FUNCTIONS
    // ==========================================
    public bool Update(SqlConnection conn, Course course, out string message)
    {
        return Update(conn, course, 0, out message);
    }

    public bool Update(SqlConnection conn, Course course, int departmentId, out string message)
    {
        if (course.Id <= 0)
        {
            message = "Validation Failed: You must provide a valid Course ID to update a record.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageCourse", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "UPDATE");
                cmd.Parameters.AddWithValue("@Id", course.Id);

                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(course.Name) ? (object)DBNull.Value : course.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(course.Name_AR) ? (object)DBNull.Value : course.Name_AR);
                cmd.Parameters.AddWithValue("@Code", string.IsNullOrEmpty(course.Code) ? (object)DBNull.Value : course.Code);

                cmd.Parameters.AddWithValue("@Credit_Hours", course.Credit_Hours == null ? (object)DBNull.Value : course.Credit_Hours);
                cmd.Parameters.AddWithValue("@Difficulty", course.Difficulty == null ? (object)DBNull.Value : course.Difficulty);
                cmd.Parameters.AddWithValue("@Status_Id", course.Status_Id == null ? (object)DBNull.Value : course.Status_Id);

                SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(statusParam);
                cmd.Parameters.Add(messageParam);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                cmd.ExecuteNonQuery();

                message = messageParam.Value?.ToString() ?? string.Empty;
                bool success = Convert.ToInt32(statusParam.Value) == 1;

                // 🌟 Manage Courses_Departments Junction Table on Update
                if (success && departmentId > 0)
                {
                    using (SqlCommand deptCmd = new SqlCommand(
                        "IF EXISTS (SELECT 1 FROM Courses_Departments WHERE Course_Id = @CourseId) " +
                        "UPDATE Courses_Departments SET Department_Id = @DepartmentId WHERE Course_Id = @CourseId; " +
                        "ELSE " +
                        "INSERT INTO Courses_Departments (Course_Id, Department_Id) VALUES (@CourseId, @DepartmentId);", conn))
                    {
                        deptCmd.Parameters.AddWithValue("@CourseId", course.Id);
                        deptCmd.Parameters.AddWithValue("@DepartmentId", departmentId);
                        deptCmd.ExecuteNonQuery();
                    }
                }

                return success;
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
            message = "Validation Failed: You must provide a valid Course ID to delete.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageCourse", conn))
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
    }

    // ==========================================
    // 4. SELECT FUNCTION
    // ==========================================
    public List<CourseDetailsView> Select(SqlConnection conn, Course searchCriteria, out string message, out int status)
    {
        message = "Data retrieved successfully.";
        status = 1;
        List<CourseDetailsView> courses = new List<CourseDetailsView>();

        try
        {
            string query = "";

            int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
            string? searchName = (searchCriteria != null) ? searchCriteria.Name : null;
            string? searchCode = (searchCriteria != null) ? searchCriteria.Code : null;

            if (searchId > 0)
            {
                query = "SELECT * FROM vw_CourseDetails WHERE Id = @SearchId";
            }
            else if (!string.IsNullOrEmpty(searchName))
            {
                query = "SELECT * FROM vw_CourseDetails WHERE Name LIKE '%' + @SearchName + '%'";
            }
            else if (!string.IsNullOrEmpty(searchCode))
            {
                query = "SELECT * FROM vw_CourseDetails WHERE Code LIKE '%' + @SearchCode + '%'";
            }
            else
            {
                query = "SELECT * FROM vw_CourseDetails";
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
                        CourseDetailsView course = new CourseDetailsView();

                        course.Id = Convert.ToInt32(reader["Id"]);
                        course.Name = reader["Name"].ToString()!;
                        course.Code = reader["Code"].ToString()!;
                        course.Credit_Hours = Convert.ToInt32(reader["Credit_Hours"]);
                        course.Difficulty = Convert.ToInt32(reader["Difficulty"]);
                        course.Status_Id = Convert.ToInt32(reader["Status_Id"]);
                        course.Description = reader["StatusDescription"].ToString()!;
                        course.DepartmentName = reader["DepartmentName"].ToString()!;
                        course.TotalStudents = Convert.ToInt32(reader["TotalStudents"]);

                        if (reader["DepartmentCode"] != DBNull.Value)
                        {
                            course.DepartmentCode = reader["DepartmentCode"].ToString()!;
                        }

                        if (reader["Name_AR"] != DBNull.Value)
                        {
                            course.Name_AR = reader["Name_AR"].ToString()!;
                        }

                        if (reader["StatusDescription_AR"] != DBNull.Value)
                        {
                            course.Description_AR = reader["StatusDescription_AR"].ToString()!;
                        }

                        courses.Add(course);
                    }
                }
            }

            return courses;
        }
        catch (SqlException)
        {
            message = "We are currently having trouble connecting to the database.";
            status = 0;
            return new List<CourseDetailsView>();
        }
        catch (Exception)
        {
            message = "An unexpected application error occurred. Please restart the application and try again.";
            status = 0;
            return new List<CourseDetailsView>();
        }
        finally
        {
            if (conn != null) conn.Close();
        }
    }
}