using DataAccessLayer.Data;
using DataAccessLayer.Interface;
using DataAccessLayer.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Repository
{
    public class LogRepository : ILogRepository
    {
        private readonly ClinicaDbContext _context;

        public LogRepository(ClinicaDbContext context)
        {
            _context = context;
        }

        public IEnumerable<LogModel> GetAllLogs()
        {
            try
            {
                var logs = _context.Set<LogModel>()
                                   .FromSqlRaw("EXEC sp_GetAllLogs")
                                   .ToList();
                return logs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter logs. Exceção: {ex.Message}");
                return new List<LogModel>();  
            }
        }

        public IEnumerable<LogModel> GetLatestEightLogs()
        {
            try
            {
                var allRelevantLogs = _context.Logs
                    .Where(log => log.Message.Contains("sent an image")
                               || log.Message.Contains("Change Password")
                               || log.Message.Contains("Update doctor")
                               || log.Message.Contains("sent PDF"))
                    .OrderByDescending(log => log.Timestamp)
                    .ThenByDescending(log => log.Id)  
                    .Take(8);

                return allRelevantLogs.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter os últimos oito logs. Exceção: {ex.Message}");
                return new List<LogModel>();  
            }
        }


        public IEnumerable<LogModel> GetLogsByDateRange(DateTime? startDate = null, DateTime? endDate = null, string userId = null)
        {
            try
            {
                var parameters = new List<SqlParameter>();

                if (startDate.HasValue)
                    parameters.Add(new SqlParameter("@StartDate", startDate.Value));

                if (endDate.HasValue)
                    parameters.Add(new SqlParameter("@EndDate", endDate.Value));

                if (!string.IsNullOrEmpty(userId))
                    parameters.Add(new SqlParameter("@UserId", userId));

                
                if (!parameters.Any())
                {
                    Console.WriteLine("Nenhum critério fornecido para a pesquisa.");
                    return new List<LogModel>();
                }

                List<LogModel> logs = new List<LogModel>();

                if (startDate.HasValue && !endDate.HasValue)
                {
                    logs = _context.Set<LogModel>()
                            .FromSqlRaw("EXEC SearchLogsByDateRange @StartDate", parameters.ToArray())
                            .ToList();
                }
                else if (!startDate.HasValue && endDate.HasValue)
                {
                    logs = _context.Set<LogModel>()
                            .FromSqlRaw("EXEC SearchLogsByDateRange @EndDate", parameters.ToArray())
                            .ToList();
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    logs = _context.Set<LogModel>()
                            .FromSqlRaw("EXEC SearchLogsByDateRange @StartDate, @EndDate", parameters.ToArray())
                            .ToList();
                }
                else if (!string.IsNullOrEmpty(userId))
                {
                    var userIdParam = new SqlParameter("@UserId", userId);
                    logs = _context.Set<LogModel>()
                            .FromSqlRaw("EXEC SearchLogsByDateRange @UserId=@UserId", userIdParam)
                            .ToList();
                }


                return logs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter logs pelo intervalo de datas. Exceção: {ex.Message}");
                return new List<LogModel>();  
            }
        }







    }
}
