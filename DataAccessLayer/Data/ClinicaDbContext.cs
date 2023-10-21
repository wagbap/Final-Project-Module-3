using DataAccessLayer.Data.Enum;
using DataAccessLayer.Filters;
using DataAccessLayer.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Data
{
    public class ClinicaDbContext : DbContext
    {
        public ClinicaDbContext()
        {

        }
        public ClinicaDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<UserModel>().HasData(
            new UserModel
            {
                UserId = 1,
                FullName = "Super Admin",
                Email = "superadmin@example.com",
                Password = "*********************".GerarHash(),
                UserType = UserTypeEnum.SuperAdmim,  
                Status = 1, 
                CreationDate = DateTime.UtcNow,
                DateATT = DateTime.UtcNow
            }
        );

           modelBuilder.Entity<Disease>().HasData(
           new Disease { Id = 1, Name = "Câncer" },
           new Disease { Id = 2, Name = "Tumor" }, 
           new Disease { Id = 3, Name = "AVC"},
           new Disease { Id = 4, Name = "Diabetes" },
           new Disease { Id = 5, Name = "Anemia" },
           new Disease { Id = 6, Name = "Hipertensão" },
           new Disease { Id = 7, Name = "Psicose" }

       );


        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Obtenha a string de conexão do arquivo de configuração específico do DAL 
                var connectionString = "Data Source=LAPTOP-D8T7SBRN;Initial Catalog=DB_eClinica;Trusted_Connection=True;TrustServerCertificate=True;"; //cn martelado
                //var connectionString = DalConfiguration.ConnectionString; // cn dinâmico
                optionsBuilder.UseSqlServer(connectionString);
            }
        }


        // Meus dbSets
        public DbSet<UserModel> Users { get; set; }
        public DbSet<Disease> Diseases { get; set; }
        public DbSet<RegionDiseaseStatistic> RegionDiseaseStatistics { get; set; }
        public DbSet<FileUser> ImgUser { get; set; }
        public DbSet<AppointmentModel> Appointments { get; set; }
        public DbSet<PatientModel> Patients { get; set; }
        public DbSet<DoctorModel> Doctors { get; set; }
        public DbSet<MessageModel> Message { get; set; }
        public DbSet<LogModel> Logs { get; set; }
    }
}
