using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Model
{
    public class DeathStatusModel : UserModel
    {

        public int UserId {get; set;}

        public bool DeathStatus { get; set; }
    }
}
