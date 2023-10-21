using DataAccessLayer.Data;
using DataAccessLayer.Data.Enum;
using DataAccessLayer.Filters;
using DataAccessLayer.Interface;
using DataAccessLayer.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public class UserRepository : IUserInterface
    {
        private readonly ClinicaDbContext _context;
        private readonly string _secretKey;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly IGenTokenFilter _genTokenFilter;
        private readonly IDecToken _decToken;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ILogger<UserRepository> logger, ClinicaDbContext context, IGenTokenFilter genTokenFilter, IDecToken decToken)
        {
            _context = context;
            _genTokenFilter = genTokenFilter;
            _secretKey = _genTokenFilter.GenerateRandomSecretKey(32);
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));
            _decToken = decToken;
            _logger = logger;
        }


        /* Doctor Sections */
        public List<DoctorModel> GetDoctorBy(string? region = null, string? city = null, string? Specialization = null)
        {
            var query = _context.Users.OfType<DoctorModel>().Where(u => !u.IsDeleted);

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(u => u.Region == region);
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(u => u.City == city);
            }

            if (!string.IsNullOrEmpty(Specialization))
            {
                query = query.Where(u => u.Especialization == Specialization);
            }

            var doctors = query.ToList();
            return doctors;
        }

        /*Metodos Genericos Users*/
        // add user
        public T AddUserGen<T>(T user) where T : UserModel
        {
            // Verifique se já existe um usuário com o mesmo email
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                // Email duplicado, retorne uma resposta de erro
                throw new ArgumentException("Email already exists.");
            }

            user.CreationDate = DateTime.Now;
            user.Status = 1;
            user.SetPasswordHash();
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        // delete user
        public bool DeleteUserGen<T>(int id) where T : UserModel
        {
            var user = _context.Users.OfType<T>().FirstOrDefault(x => x.UserId == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        // soft delete user
        public bool SoftDeleteUserGen<T>(int id) where T : UserModel
        {
            var user = _context.Users.OfType<T>().FirstOrDefault(x => x.UserId == id);
            if (user != null)
            {
                user.IsDeleted = true;
                user.Status = 0;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        // restore soft-deleted user
        public bool RestoreDeletedUserGen<T>(int id) where T : UserModel
        {
            var user = _context.Users.OfType<T>().FirstOrDefault(x => x.UserId == id && x.IsDeleted);
            if (user != null)
            {
                user.IsDeleted = false;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        // get all users (excluding soft-deleted ones)
        public List<T> GetAllUsersGen<T>() where T : UserModel
        {
            var users = _context.Users.OfType<T>().Where(u => !u.IsDeleted).ToList();
            return users;
        }

        // get all soft-deleted users
        public List<T> GetAllDeletedUsersGen<T>() where T : UserModel
        {
            var users = _context.Users.OfType<T>().Where(u => u.IsDeleted).ToList();
            return users;
        }

        // get user by id (excluding soft-deleted ones)
        public T GetUserByIdGen<T>(int id) where T : UserModel
        {
            var user = _context.Users.OfType<T>().FirstOrDefault(x => x.UserId == id && !x.IsDeleted);
            return user;
        }

        // get doctor by email (excluding soft-deleted ones)
        public T GetUserByEmailGen<T>(string email) where T : UserModel
        {
            var user = _context.Users.OfType<T>().FirstOrDefault(x => x.Email.ToLower() == email.ToLower() && !x.IsDeleted);
            return user;
        }

        // Update user
        public T UpdateUserGen<T>(T user) where T : UserModel
        {
            var existingUser = GetUserByIdGen<T>(user.UserId);
            if (existingUser == null)
            {
                throw new ArgumentException($"User with ID {user.UserId} not found.");
            }

            foreach (var property in typeof(T).GetProperties())
            {
                if (property.Name != "UserId" && property.CanWrite && property.Name != "Password")
                {
                    property.SetValue(existingUser, property.GetValue(user));
                }
            }

            existingUser.DateATT = DateTime.Now;

            _context.SaveChanges();

            return existingUser;
        }




        public void UpdateDeathStatus(int userId, bool deathStatus)
        {
            var user = _context.RegionDiseaseStatistics.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException($"No user found with ID {userId}.");
            }

            user.DeathStatus = deathStatus;
            _context.SaveChanges();
        }


        // Get all Image url by userId and return image path
        public List<FileUser> GetImage()
        {
            var userImg = _context.ImgUser.ToList();
            return userImg;
        }

        
        public bool IsFileCopy(FileUser image, int userId, int? appoint = null)
        {
            if (image != null)
            {
                var imageUser = new FileUser
                {
                    ImageUrl = image.ImageUrl,
                    UserId = userId
                };
                _context.ImgUser.Add(imageUser);
                _context.SaveChanges();

                if (appoint != null)
                {
                    var pdfFile = _context.Appointments.FirstOrDefault(x => x.AppointId == appoint);
                    pdfFile.PDFFile = imageUser.ImgId.ToString();
                    _context.Appointments.Update(pdfFile);
                    _context.SaveChanges();
                }

                return true;
            }

            return false;
        }
        public bool RegisterRegionDiseaseStatistic(int userPatientId, int diseaseId, string region)
        {
            
            if (userPatientId <= 0)
                throw new ArgumentException("Invalid user ID.");

            if (diseaseId <= 0)
                throw new ArgumentException("Invalid disease ID.");

            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentException("Region cannot be null or empty.");

            
            var userExists = _context.Users.Any(u => u.UserId == userPatientId);
            if (!userExists)
                throw new ArgumentException("User with the given ID does not exist.");

            
            var diseaseExists = _context.Diseases.Any(d => d.Id == diseaseId);
            if (!diseaseExists)
                throw new ArgumentException("Disease with the given ID does not exist.");

            
            var regionDiseaseStatistic = new RegionDiseaseStatistic
            {
                UserId = userPatientId,
                DiseaseId = diseaseId,
                Region = region,
                DeathStatus = false 
            };

           
            _context.RegionDiseaseStatistics.Add(regionDiseaseStatistic);
            _context.SaveChanges();

            return true;
        }


        public List<DiseaseWithStatisticsVM> GetAllDiseasesWithStatistics()
        {
            var data = _context.RegionDiseaseStatistics
                .Join(
                    _context.Diseases,
                    rds => rds.DiseaseId,
                    disease => disease.Id,
                    (rds, disease) => new { RegionDiseaseStat = rds, Disease = disease }
                )
                .Join(
                    _context.Users,
                    rdsWithDisease => rdsWithDisease.RegionDiseaseStat.UserId,
                    user => user.UserId,
                    (rdsWithDisease, user) => new { rdsWithDisease.RegionDiseaseStat, rdsWithDisease.Disease, User = user }
                )
                .Select(joined => new DiseaseWithStatisticsVM
                {
                    DiseaseId = joined.Disease.Id,
                    UserId = joined.RegionDiseaseStat.UserId,
                    Id = joined.RegionDiseaseStat.Id,
                    DiseaseName = joined.Disease.Name,
                    Region = joined.RegionDiseaseStat.Region,
                    DeathStatus = joined.RegionDiseaseStat.DeathStatus,
                    FullName = joined.User.FullName 
                })
                .ToList();

            return data;
        }



        public List<Disease> GetAllDiseases()
        {
            return _context.Diseases.ToList();
        }
    }
}
