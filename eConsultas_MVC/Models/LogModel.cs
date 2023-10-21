namespace eConsultas_MVC.Models
{
    public class LogModel
    {
        public int? Id { get; set; } // Embora geralmente o ID não seja nullable
        public string Message { get; set; }
        public string Level { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? UserId { get; set; }
        public string Obs { get; set; }

        public string? Exception { get; set; }  // Strings já são naturalmente nullable


    }
}
