using Microsoft.Data.SqlClient;
using Registration.Domain;
using System.Collections.Generic;
using System.Data;
using System;

namespace Registration.DataAccess
{
    public class GradeOperations
    {
        // ==========================================
        // 1. SAVE OR UPDATE GRADE (UPSERT)
        // ==========================================
        public bool SaveGrade(SqlConnection conn, int enrollmentId, decimal? midterm, decimal? coursework, decimal? final, out string message)
        {
            if (enrollmentId <= 0)
            {
                message = "Validation Failed: You must provide a valid Enrollment ID to save grades.";
                return false;
            }

            message = string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_ManageGrade", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Set the action to UPSERT
                    cmd.Parameters.AddWithValue("@Action", "UPSERT");
                    cmd.Parameters.AddWithValue("@Enrollment_Id", enrollmentId);

                    // 2. Pass grade parameters (handle nulls with DBNull)
                    cmd.Parameters.AddWithValue("@Midterm_Grade", midterm == null ? (object)DBNull.Value : midterm);
                    cmd.Parameters.AddWithValue("@CourseWork_Grade", coursework == null ? (object)DBNull.Value : coursework);
                    cmd.Parameters.AddWithValue("@Final_Grade", final == null ? (object)DBNull.Value : final);

                    // 3. Output Parameters for Stored Procedure
                    SqlParameter statusParam = new SqlParameter("@ProcessingStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    SqlParameter messageParam = new SqlParameter("@ProcessingMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    // 4. Open connection and execute
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    cmd.ExecuteNonQuery();

                    // 5. Unpack results from Stored Procedure
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
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }

        // ==========================================
        // 2. SELECT FUNCTION (USING THE SQL VIEW)
        // ==========================================
        public List<GradeDetailsView> GetGradesByCourse(SqlConnection conn, int courseId, out string message, out int status)
        {
            message = "Data retrieved successfully.";
            status = 1;
            List<GradeDetailsView> gradesList = new List<GradeDetailsView>();

            try
            {
                string query = "SELECT * FROM vw_CourseGrades WHERE CourseId = @CourseId";

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
                            GradeDetailsView gradeView = new GradeDetailsView();

                            gradeView.EnrollmentId = Convert.ToInt32(reader["EnrollmentId"]);
                            gradeView.StudentId = Convert.ToInt32(reader["StudentId"]);
                            gradeView.StudentName = reader["StudentName"].ToString()!;

                            if (reader["MidtermGrade"] != DBNull.Value)
                                gradeView.MidtermGrade = Convert.ToDecimal(reader["MidtermGrade"]);

                            if (reader["CourseWorkGrade"] != DBNull.Value)
                                gradeView.CourseWorkGrade = Convert.ToDecimal(reader["CourseWorkGrade"]);

                            if (reader["FinalGrade"] != DBNull.Value)
                                gradeView.FinalGrade = Convert.ToDecimal(reader["FinalGrade"]);

                            if (reader["TotalGrade"] != DBNull.Value)
                                gradeView.TotalGrade = Convert.ToDecimal(reader["TotalGrade"]);

                            // If you added the LetterGrade column to your vw_CourseGrades view, 
                            // you can map it here if your GradeDetailsView supports it!

                            gradesList.Add(gradeView);
                        }
                    }
                }

                return gradesList;
            }
            catch (SqlException)
            {
                message = "We are currently having trouble connecting to the database.";
                status = 0;
                return new List<GradeDetailsView>();
            }
            catch (Exception)
            {
                message = "An unexpected application error occurred. Please restart the application and try again.";
                status = 0;
                return new List<GradeDetailsView>();
            }
            finally
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}