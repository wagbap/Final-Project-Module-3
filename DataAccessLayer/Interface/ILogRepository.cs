using DataAccessLayer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interface
{
    public interface ILogRepository
    {
        IEnumerable<LogModel> GetAllLogs();
        
        IEnumerable<LogModel> GetLatestEightLogs();

        IEnumerable<LogModel> GetLogsByDateRange(DateTime? startDate, DateTime? endDate, string userId = null);
    }

}
