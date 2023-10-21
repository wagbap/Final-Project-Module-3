using DataAccessLayer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Model
{
    [JsonObject]
    public class PatientModel : UserModel
    {

        public string? Region { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }

    }
}
