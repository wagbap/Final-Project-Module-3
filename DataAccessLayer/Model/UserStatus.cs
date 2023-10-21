using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Model
{
    public class UserStatus : UserModel
    {

        public int DoctorUserId { get; set; }

        public int PatientUserId  { get; set; }  

        public int Status { get; set;}
    }
}
