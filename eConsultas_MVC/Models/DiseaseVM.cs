using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace eConsultas_MVC.Models
{
    

    public class DiseaseVM
    {
       
   
        public int UserPatientId { get; set; }
        public IEnumerable<SelectListItem> Patients { get; set; }

        public string Region { get; set; }
        public IEnumerable<SelectListItem> Regions { get; set; }

        public int DiseaseId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }
        public IEnumerable<SelectListItem> Diseases { get; set; }

        public string FullName { get; set; }

        public bool DeathStatus { get; set; }

        public IEnumerable<DiseaseStatisticMV> DiseasesStatistics { get; set; }

        public class DiseaseStatisticMV
        {


            public int Id { get; set; }
            public int UserId { get; set; }
            public string FullName { get; set; }
            public string DiseaseName { get; set; }
            public string Region { get; set; }
            public bool DeathStatus { get; set; }
         
        }
    }
}
