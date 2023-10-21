using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Model;
using Microsoft.Extensions.Logging; // Adicionado para ILogger
using DataAccessLayer.Interface;
using DataAccessLayer.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Data;
using System.Net;

namespace eConsultas_API.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IDecToken _decToken;
        private readonly ILoginInterface _loginRepository;
        private readonly ClinicaDbContext _context;

        public LoginController(ILogger<UsersController> logger, IDecToken decToken, ILoginInterface loginRepository, ClinicaDbContext context)
        {
            _logger = logger;
            _decToken = decToken;
            _loginRepository = loginRepository;
            _context = context;
        }


        /* Login Section */

        [HttpPost]
        public IActionResult Login([FromBody] LoginModel login)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var isLog = _loginRepository.logIn(login);
                    if (isLog != null)
                    {
                        return Ok(isLog);
                    }
                    else
                    {
                        return NotFound("Invalid user or password");
                    }
                }
                else
                    return NotFound("Invalid user or password");
            }
            catch (Exception)
            {

                throw;
            }
        }


        [UserAcess]
        [HttpPost]
        public IActionResult GetUserLogged([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                // Extrai o token do cabeçalho
                string token = authorizationHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    return Ok(_decToken.GetLoggedUser(token));
                }
            }
            return BadRequest("Invalid token");
        }

        [UserAcess]
        [HttpPost]
        public IActionResult ChangePwd([FromBody] ChangePwdModel changePwd, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrEmpty(token)) return BadRequest("Invalid token");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loggedUser = _decToken.GetLoggedUser(token);

            if (loggedUser == null) return NotFound("Invalid user or password");

            var user = _context.Users.FirstOrDefault(x => x.UserId == loggedUser.UserId);

            if (user == null) return NotFound("User not found");

            changePwd.UserId = user.UserId;
            string msg = "Change Password: " + user.UserId;
            if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");

            return Ok(_loginRepository.ChangePwd<UserModel>(changePwd));
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPwd([FromBody] ForgotPwdModel forgotPwd)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _loginRepository.ForgotPwd<UserModel>(forgotPwd));
        }


        private string GetClientIpAddress(HttpContext context)
        {
            
            string clientIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();

            // Se CF-Connecting-IP não está presente, verificar X-Forwarded-For
            if (string.IsNullOrEmpty(clientIp))
            {
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    clientIp = forwardedFor.Split(',').FirstOrDefault();
                }
            }

            // Se ainda não temos um IP, vamos pegar o IP remoto padrão
            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = context.Connection.RemoteIpAddress?.ToString();
            }

            // Se o IP é ::1 (localhost IPv6), converta para 127.0.0.1 (localhost IPv4)
            if (clientIp == "::1")
            {
                clientIp = "127.0.0.1";
            }

            return clientIp;
        }

        /*logger*/

        private bool loggers(string msg, string authorizationHeader)
        {
            //string ipAddress = context.Connection.RemoteIpAddress?.ToString();
            string machineName = Dns.GetHostName();

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                // Extrai o token do cabeçalho
                string token = authorizationHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    var user = _decToken.GetLoggedUser(token);
                    if (user == null) return false;

                    string userId = user.FullName + " - " + user.UserId.ToString();
                    string obs = msg;
                    var clientIp = GetClientIpAddress(this.HttpContext);
                    _logger.LogInformation("User: {UserId} accessed {Obs} from IP: {IpAddress} on machine: {MachineName}", userId, obs, clientIp, machineName);

                    return true;
                }
            }
            return false;
        }

    }


}
