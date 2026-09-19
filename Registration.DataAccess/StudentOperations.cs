using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Registration.Domain;
using Registration.Domain.Enums;

namespace Registration.DataAccess;

public class StudentOperations
{
    // ==========================================
    // 1. CREATE FUNCTION
    // ==========================================
    public bool Create(SqlConnection conn, Student student, out string message)
    {
        message = string.Empty;

        //try and catch here for error before connecting to the database 

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageStudent", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "INSERT");
                cmd.Parameters.AddWithValue("@Name", student.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(student.Name_AR) ? (object)DBNull.Value : student.Name_AR);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                cmd.Parameters.AddWithValue("@Password", student.Password);
                cmd.Parameters.AddWithValue("@Gpa", student.Gpa == null ? (object)DBNull.Value : student.Gpa);
                cmd.Parameters.AddWithValue("@Year", student.Year == null ? (object)DBNull.Value : student.Year);
                cmd.Parameters.AddWithValue("@National_Id", student.National_Id);
                cmd.Parameters.AddWithValue("@DepartmentCode", student.DepartmentCode);
                cmd.Parameters.AddWithValue("@StatusDescription", student.StatusDescription);

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
            // You (the developer) might want to log 'ex.Message' to a text file for debugging,
            // but the user just gets a polite apology:
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
    public bool Update(SqlConnection conn, Student student, out string message)
    {
        if (student.Id <= 0)
        {
            message = "Validation Failed: You must provide a valid Student ID to update a record.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageStudent", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // 1. Set the action to UPDATE
                cmd.Parameters.AddWithValue("@Action", "UPDATE");

                // 2. CRITICAL: Pass the ID so SQL Server knows which row to update
                cmd.Parameters.AddWithValue("@Id", student.Id);

                // 3. String parameters: Use the DBNull trick to trigger your COALESCE in SQL!
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(student.Name) ? (object)DBNull.Value : student.Name);
                cmd.Parameters.AddWithValue("@Name_AR", string.IsNullOrEmpty(student.Name_AR) ? (object)DBNull.Value : student.Name_AR);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(student.Email) ? (object)DBNull.Value : student.Email);
                cmd.Parameters.AddWithValue("@Password", string.IsNullOrEmpty(student.Password) ? (object)DBNull.Value : student.Password);
                cmd.Parameters.AddWithValue("@National_Id", string.IsNullOrEmpty(student.National_Id) ? (object)DBNull.Value : student.National_Id);
                // Pass the string codes from your updated Student class
                cmd.Parameters.AddWithValue("@DepartmentCode", string.IsNullOrEmpty(student.DepartmentCode) ? (object)DBNull.Value : student.DepartmentCode);
                cmd.Parameters.AddWithValue("@StatusDescription", string.IsNullOrEmpty(student.StatusDescription) ? (object)DBNull.Value : student.StatusDescription);
                // 4. Non-string parameters
                cmd.Parameters.AddWithValue("@Gpa", student.Gpa == null ? (object)DBNull.Value : student.Gpa);
                cmd.Parameters.AddWithValue("@Year", student.Year == null ? (object)DBNull.Value : student.Year);


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
            // You (the developer) might want to log 'ex.Message' to a text file for debugging,
            // but the user just gets a polite apology:
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
        // ==========================================
        // 1. THE GUARD CLAUSE
        // ==========================================
        if (id <= 0)
        {
            message = "Validation Failed: You must provide a valid Student ID to delete.";
            return false;
        }

        message = string.Empty;

        try
        {
            using (SqlCommand cmd = new SqlCommand("usp_ManageStudent", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // 2. We only need to send TWO input parameters!
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@Id", id);

                // 3. Set up the empty boxes for SQL to send answers back
                SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(statusParam);
                cmd.Parameters.Add(messageParam);

                // 4. Open connection and execute
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                cmd.ExecuteNonQuery();

                // 5. Read the database manager's receipt
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

    public List<StudentDetailsView> Select(SqlConnection conn, Student searchCriteria, out string message, out int status)
    {
        message = "Data retrieved successfully.";
        status = 1;
        List<StudentDetailsView> students = new List<StudentDetailsView>();

        try
        {
            // 1. Declare a base query variable
            string query = "";

            // 2. Check the object properties in C# to decide which query to use
            int searchId = (searchCriteria != null) ? searchCriteria.Id : 0;
            string? searchName = (searchCriteria != null) ? searchCriteria.Name : null;
            string? searchEmail = (searchCriteria != null) ? searchCriteria.Email : null;

            if (searchId > 0)
            {
                query = "SELECT * FROM vw_Students WHERE Id = @SearchId";
            }
            else if (!string.IsNullOrEmpty(searchName))
            {
                query = "SELECT * FROM vw_Students WHERE StudentName LIKE '%' + @SearchName + '%'";
            }
            else if (!string.IsNullOrEmpty(searchEmail))
            {
                query = "SELECT * FROM vw_Students WHERE Email LIKE '%' + @SearchEmail + '%'";
            }
            else
            {
                query = "SELECT * FROM vw_Students"; // Returns everyone if all are empty
            }

            // 3. Pass the dynamic query into the SqlCommand using your 'using' block
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;

                // Pass the parameters safely
                cmd.Parameters.AddWithValue("@SearchId", searchId > 0 ? (object)searchId : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchName", !string.IsNullOrEmpty(searchName) ? (object)searchName : DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchEmail", !string.IsNullOrEmpty(searchEmail) ? (object)searchEmail : DBNull.Value);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StudentDetailsView student = new StudentDetailsView();

                        student.Id = Convert.ToInt32(reader["Id"]);
                        student.StudentName = reader["StudentName"].ToString()!;
                        student.Email = reader["Email"].ToString()!;
                        student.Password = reader["Password"].ToString()!;
                        student.National_Id = reader["National_Id"].ToString()!;
                        student.DepartmentName = reader["DepartmentName"].ToString()!;
                        student.DepartmentCode = reader["DepartmentCode"].ToString()!;
                        student.Description = reader["Description"].ToString()!;

                        // Read the comma-separated string from SQL and split it into the new List property
                        string coursesRaw = reader["EnrolledCourses"].ToString()!;
                        if (!string.IsNullOrEmpty(coursesRaw))
                        {
                            student.EnrolledCoursesList = coursesRaw.Split(", ").ToList();
                        }

                        // Read the enrollment statuses and split them into the status list
                        string statusesRaw = reader["EnrollmentStatuses"].ToString()!;
                        if (!string.IsNullOrEmpty(statusesRaw))
                        {
                            student.EnrollmentStatusList = statusesRaw.Split(", ").ToList();
                        }

                        // Read individual grade component strings and split them into lists
                        string midtermRaw = reader["MidtermGrades"].ToString()!;
                        if (!string.IsNullOrEmpty(midtermRaw))
                        {
                            student.MidtermGradesList = midtermRaw.Split(", ").ToList();
                        }

                        string courseworkRaw = reader["CourseWorkGrades"].ToString()!;
                        if (!string.IsNullOrEmpty(courseworkRaw))
                        {
                            student.CourseWorkGradesList = courseworkRaw.Split(", ").ToList();
                        }

                        string finalRaw = reader["FinalGrades"].ToString()!;
                        if (!string.IsNullOrEmpty(finalRaw))
                        {
                            student.FinalGradesList = finalRaw.Split(", ").ToList();
                        }

                        string gradesRaw = reader["TotalGrades"].ToString()!;
                        if (!string.IsNullOrEmpty(gradesRaw))
                        {
                            student.TotalGradesList = gradesRaw.Split(", ").ToList();
                        }

                        student.CourseCodes = reader["CourseCodes"].ToString()!;
                        student.Year = Convert.ToInt32(reader["Year"]);

                        if (reader["StudentName_AR"] != DBNull.Value)
                        {
                            student.StudentName_AR = reader["StudentName_AR"].ToString()!;
                        }

                        if (reader["Gpa"] != DBNull.Value)
                        {
                            student.Gpa = Convert.ToSingle(reader["Gpa"]);
                        }

                        students.Add(student);
                    }
                }
            }

            return students;
        }
        catch (SqlException)
        {
            message = "We are currently having trouble connecting to the database.";
            status = 0;
            return new List<StudentDetailsView>();
        }
        catch (Exception)
        {
            message = "An unexpected application error occurred. Please restart the application and try again.";
            status = 0;
            return new List<StudentDetailsView>();
        }
        finally
        {
            if (conn != null) conn.Close();
        }
    }
    public bool UpdateStatusOnly(SqlConnection conn, int id, string statusDescription, out string message)
    {
        try
        {
            // Add this check to fix the Enum vs Database spelling mismatch
            if (statusDescription == "OnLeave")
            {
                statusDescription = "On Leave";
            }

            // NOTE: If you are putting this in InstructorOperations.cs, 
            // remember to change "Students" to "Instructors" and "StudentsStatus" to your instructor status table!
            string query = @"UPDATE Students 
                         SET Status_Id = (SELECT Id FROM StudentsStatus WHERE Description = @Status OR Name = @Status) 
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
                    message = "Status updated successfully.";
                    return true;
                }
                message = "Student not found with ID " + id;
                return false;
            }
        }
        catch (SqlException)
        {
            // Catches database-specific errors (like connection drops or missing tables)
            message = "We are currently having trouble connecting to the database.";
            return false;
        }
        catch (Exception)
        {
            // Catches all other general C# application errors
            message = "An unexpected application error occurred. Please restart the application and try again.";
            return false;
        }
        finally
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Open)
                conn.Close();
        }
    }

    public List<StudentScheduleView> GetStudentSchedule(SqlConnection conn, int studentId, out string message)
    {
        message = string.Empty;
        List<StudentScheduleView> schedule = new List<StudentScheduleView>();

        try
        {
            // Join the tables to find exactly when and where the student's enrolled courses are happening
            string query = @"
            SELECT 
            c.Name AS CourseName,
            c.Code AS CourseCode,
            cs.Day,
            cs.Type,
            cs.StartTime,
            cs.EndTime,
            i.Name AS InstructorName
            FROM Enrollments e
            INNER JOIN Courses c ON e.Course_Id = c.Id
            INNER JOIN Instructors_Courses ic ON c.Id = ic.Course_Id
            INNER JOIN Course_Schedules cs ON ic.Id = cs.Instructors_Courses_Id
            LEFT JOIN Instructors i ON ic.Instructor_Id = i.Id
            WHERE e.Student_Id = @StudentId
             -- FIXED: Now it ONLY grabs courses where the status is 'Enrolled' or 'Active'
             AND e.Status_Id IN (SELECT Id FROM EnrollmentsStatus WHERE Description IN ('Enrolled', 'Active'))
            ORDER BY 
            CASE cs.Day 
            WHEN 'Saturday' THEN 1
            WHEN 'Sunday' THEN 2 
            WHEN 'Monday' THEN 3 
            WHEN 'Tuesday' THEN 4 
            WHEN 'Wednesday' THEN 5 
            WHEN 'Thursday' THEN 6 
            WHEN 'Friday' THEN 7 
            END, 
            cs.StartTime";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add(new StudentScheduleView
                        {
                            CourseName = reader["CourseName"].ToString()!,
                            CourseCode = reader["CourseCode"].ToString()!,
                            Day = reader["Day"].ToString()!,
                            Type = reader["Type"].ToString()!,
                            StartTime = (TimeSpan)reader["StartTime"],
                            EndTime = (TimeSpan)reader["EndTime"],
                            InstructorName = reader["InstructorName"] != DBNull.Value ? reader["InstructorName"].ToString()! : "TBA"
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            message = "Failed to load schedule: " + ex.Message;
        }
        finally
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Open)
                conn.Close();
        }

        return schedule;
    }
    public List<StudentTranscriptView> GetStudentTranscript(SqlConnection conn, int studentId, out string message)
    {
        message = string.Empty;
        List<StudentTranscriptView> transcript = new List<StudentTranscriptView>();

        try
        {
            string query = @"
            SELECT 
                COALESCE(c.Name, '') AS CourseName,
                COALESCE(c.Code, '') AS CourseCode,
                s.Name AS SemesterName,
                COALESCE(es.Description, '-') AS StatusDescription,
                g.Total_Grade AS TotalGrade
            FROM Semesters s
            LEFT JOIN Enrollments e ON s.Id = e.Semester_Id AND e.Student_Id = @StudentId
            LEFT JOIN Courses c ON e.Course_Id = c.Id
            LEFT JOIN EnrollmentsStatus es ON e.Status_Id = es.Id
            LEFT JOIN Grades g ON e.Id = g.Enrollment_Id
            WHERE s.Id <= (SELECT MAX(ISNULL(Semester_Id, s.Id)) FROM Enrollments WHERE Student_Id = @StudentId)
            ORDER BY s.Id ASC, c.Name";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transcript.Add(new StudentTranscriptView
                        {
                            CourseName = reader["CourseName"].ToString()!,
                            CourseCode = reader["CourseCode"].ToString()!,
                            SemesterName = reader["SemesterName"].ToString()!,
                            StatusDescription = reader["StatusDescription"].ToString()!,
                            TotalGrade = reader["TotalGrade"] != DBNull.Value ? Convert.ToDecimal(reader["TotalGrade"]) : (decimal?)null
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            message = "Failed to load transcript: " + ex.Message;
        }
        finally
        {
            if (conn != null && conn.State == ConnectionState.Open)
                conn.Close();
        }

        return transcript;
    }
}