using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Model
{
    [Table("Logs")]
    public class LogModel
    {
        public int? Id { get; set; } // Embora geralmente o ID não seja nullable
        public string Message { get; set; }
        public string Level { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? UserId { get; set; }
        public string Obs { get; set; }
        public string? Exception { get; set; }  // Strings já são naturalmente nullable
        public string? Properties     { get; set;}
       
    }

}
