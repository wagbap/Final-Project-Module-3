using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Model
{
    public class DiseaseWithStatisticsVM
    {

        public int Id { get; set; }

        public int UserId { get; set; } 
        public int DiseaseId { get; set; }
        public string DiseaseName { get; set; }
        public string Region { get; set; }
        public bool DeathStatus { get; set; }
        public string FullName { get; set; }
      
    }
}
