using LibBiz.Interfaces;
using LibBiz.Data;
using LibBiz.Models;
using Microsoft.Extensions.Logging;


namespace LibBiz.Repositories
{
    public class RepositoryAppUser : IRepository<User>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger _logger;

        public RepositoryAppUser(AppDbContext appDbContext, ILogger<User> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task<User> Create(User appuser)
        {
            try
            {
                if (appuser != null)
                {
                    var obj = _appDbContext.Add(appuser);
                    await _appDbContext.SaveChangesAsync();
                    return obj.Entity;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return null;
        }

        public async Task Delete(User appuser)
        {
            try
            {
                if (appuser != null)
                {
                    _appDbContext.Remove(appuser);
                    await _appDbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<User> GetAll()
        {
            try
            {
                return _appDbContext.Users.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public User GetById(int Id)
        {
            try
            {
                return _appDbContext.Users.FirstOrDefault(x => x.Id == Id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task Update(User appuser)
        {
            try
            {
                if (appuser != null)
                {
                    _appDbContext.Update(appuser);
                    await _appDbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        void IRepository<User>.Delete(User _object)
        {
            throw new NotImplementedException();
        }

        void IRepository<User>.Update(User _object)
        {
            throw new NotImplementedException();
        }
    }
}
