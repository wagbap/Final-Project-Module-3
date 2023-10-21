namespace eConsultas_MVC.Models
{
    public class PatientMV : UserMV
    {

        public int UserId { get; set; }
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Status { get; set; }

        public string Region { get; set; }
    }
}
