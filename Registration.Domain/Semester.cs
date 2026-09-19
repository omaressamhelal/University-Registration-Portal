using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registration.Domain
{
    public class Semester
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Fixes the warning
        public string? Name_AR { get; set; }            // Fixes the warning by allowing nulls
        public DateTime? Start_date { get; set; }
        public DateTime? End_date { get; set; }
        public bool? Is_Registration_Open { get; set; }
    }
}
