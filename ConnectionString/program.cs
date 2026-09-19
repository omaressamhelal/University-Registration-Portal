using Microsoft.Data.SqlClient;
using Registration.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectionString
{
    public class program
    {
        public static void Main()
        {
            StudentOperation student = new StudentOperation();
            string constr = @"Server=.\SQLEXPRESS;  Database = Universityportal;  Integrated Security = SSPI; TrustServerCertificate=True";
            SqlConnection conn = new SqlConnection(constr);
            Console.WriteLine(constr);
            Console.ReadKey();
        }

    }
}
