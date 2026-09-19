namespace Registration.Domain
{
    public class AdminDetailsView
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string National_id { get; set; } = string.Empty;
        public int Start_year { get; set; }
        public double Salary { get; set; }
        public string Role { get; set; } = string.Empty;
        public int Status_id { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
    }
}