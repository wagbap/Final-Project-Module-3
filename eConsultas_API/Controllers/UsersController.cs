using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Model;
using Microsoft.Extensions.Logging; // Adicionado para ILogger
using DataAccessLayer.Interface;
using DataAccessLayer.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using DataAccessLayer.Data.Enum;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Net;
using DataAccessLayer.Repository;

namespace eConsultas_API.Controllers
{

    [ApiController]
    [Route("[controller]/[Action]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserInterface _userRepository;
        private readonly IDecToken _decToken;
        private readonly ILogRepository _logsRepository; // Suponhamos que isso exista

        public UsersController(ILogger<UsersController> logger, IUserInterface userRepository, IDecToken decToken, ILogRepository logsRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _decToken = decToken;
            _logsRepository = logsRepository;
        }

        [HttpGet]
        [PrivilegeUser("SuperAdmim")]
        [UserAcess]
        public IActionResult GetAllLogs()
        {
            try
            {
                var logs = _logsRepository.GetAllLogs();  // Suponha que este método retorna todos os logs.
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os logs.");
                return BadRequest("Ocorreu um erro ao buscar os logs.");
            }
        }

        [HttpGet]
        public IActionResult GetLatestEightLogs()   
        {
            try
            {
                var logs = _logsRepository.GetLatestEightLogs();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os últimos seis logs.");
                return BadRequest("Ocorreu um erro ao buscar os últimos seis logs.");
            }
        }



        // Método GET para buscar logs por um intervalo de datas
        [HttpGet]
        public IActionResult GetLogsByDateRange(DateTime? startDate, DateTime? endDate, string userId = null)
        {
            try
            {
                var logs = _logsRepository.GetLogsByDateRange(startDate, endDate, userId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                // Aqui você pode logar a exceção se desejar
                return BadRequest($"Erro ao obter logs: {ex.Message}");
            }
        }


        /* Doctor Sections */
        [HttpGet]
        [PrivilegeUser("SuperAdmim")]
        public IActionResult GetDoctorById(int doctorId, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            return Ok(_userRepository.GetUserByIdGen<PatientModel>(doctorId));
        }



        [HttpGet]
        public IActionResult DoctorBy(string? region = null, string? city = null, string? Specialization = null)
        {
            return Ok(_userRepository.GetDoctorBy(region, city, Specialization));
        }

        [HttpGet]
        [UserAcess]
        public IActionResult GetAllDoctor([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            return Ok(_userRepository.GetAllUsersGen<DoctorModel>());
        }

        [HttpPost]
        public IActionResult AddDoctor([FromBody] DoctorModel doctor) //, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            try
            {
                //string msg = "Try to Add Doctor" + doctor.FullName;
                //if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
                if (ModelState.IsValid)
                {
                    doctor.UserType = UserTypeEnum.Doctor;
                    return Ok(_userRepository.AddUserGen(doctor));
                }
                else
                {
                    return BadRequest("Bad request");
                }
            }
            catch (ArgumentException ex)
            {
                // Capturando a exceção de email duplicado e retornando uma resposta de erro apropriada
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [PrivilegeUser("SuperAdmim")]
        public IActionResult DeleteDoctor(int doctorId, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            var user = _userRepository.GetUserByIdGen<DoctorModel>(doctorId);
            string msg = "Delet doctor: " + doctorId;
            if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
            if (user == null)
            {
                return NotFound();
            }

            return Ok(_userRepository.DeleteUserGen<DoctorModel>(user.UserId));
        }

        [HttpGet]
        [PrivilegeUser("SuperAdmim")]
        public IActionResult GetDoctorByEmail(string email)
        {
            return Ok(_userRepository.GetUserByEmailGen<DoctorModel>(email));
        }


        [HttpPost]
        [PrivilegeUser("Doctor")]

        public IActionResult UpdateDoctor([FromHeader(Name = "Authorization")] string authorizationHeader, [FromBody] DoctorUpdateInfo doctor)
        {

            // Verifique se o token é válido e obtenha o usuário logado.
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();
            var userLogged = _decToken.GetLoggedUser(token);
            if (userLogged == null)
            {
                return BadRequest("Token inválido ou usuário não encontrado.");
            }

            // Verifique se o usuário a ser atualizado existe.
            var userToUpdate = _userRepository.GetUserByIdGen<DoctorModel>(userLogged.UserId);
            if (userToUpdate == null)
            {
                return BadRequest("Usuário a ser atualizado não encontrado.");
            }

            // Atualize as propriedades do usuário com os valores do objeto "doctor".
            if (doctor.Especialization != null) userToUpdate.Especialization = doctor.Especialization;
            if (doctor.Fees != null) userToUpdate.Fees = doctor.Fees;
            if (doctor.AdInfo != null) userToUpdate.AdInfo = doctor.AdInfo;
            if (doctor.Region != null) userToUpdate.Region = doctor.Region;
            if (doctor.City != null) userToUpdate.City = doctor.City;
            if (doctor.Address != null) userToUpdate.Address = doctor.Address;
            if (doctor.Status != null) userToUpdate.Status = (int)doctor.Status;





            if (userToUpdate != null)
            {
                string msg = "Update doctor" + doctor.DoctorUserId;
                if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
                return Ok(_userRepository.UpdateUserGen<DoctorModel>(userToUpdate));
            }
            else
                return NotFound();
        }

        [HttpPost]
        public IActionResult UpdateUserStatus(int? doctorUserId, int? patientUserId, int status)
        {
            // Se doctorUserId for fornecido, tente atualizar o status do médico.
            if (doctorUserId.HasValue)
            {
                var doctorToUpdate = _userRepository.GetUserByIdGen<UserModel>(doctorUserId.Value);
                if (doctorToUpdate == null)
                {
                    return BadRequest($"Médico com ID {doctorUserId} não encontrado.");
                }
                doctorToUpdate.Status = status;
                _userRepository.UpdateUserGen(doctorToUpdate);
            }

            // Se patientUserId for fornecido, tente atualizar o status do paciente.
            if (patientUserId.HasValue)
            {
                var patientToUpdate = _userRepository.GetUserByIdGen<UserModel>(patientUserId.Value);
                if (patientToUpdate == null)
                {
                    return BadRequest($"Paciente com ID {patientUserId} não encontrado.");
                }
                patientToUpdate.Status = status;
                _userRepository.UpdateUserGen(patientToUpdate);
            }

            // Se nenhum ID foi fornecido, retorne um erro.
            if (!doctorUserId.HasValue && !patientUserId.HasValue)
            {
                return BadRequest("Nenhum ID de médico ou paciente fornecido.");
            }

            return Ok("Status atualizado com sucesso.");
        }




        [HttpPost]
        public IActionResult UpdateDeaphStatus(int? patientUserId, bool status)
        {
            bool isDead = status == true; // Assuming 1 means dead

   

            if (patientUserId.HasValue)
            {
                try
                {
                    _userRepository.UpdateDeathStatus(patientUserId.Value, isDead);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest($"Paciente com ID {patientUserId} não encontrado.");
                }
            }

            if (!patientUserId.HasValue)
            {
                return BadRequest("Nenhum ID de médico ou paciente fornecido.");
            }

            return Ok("DeaphStatus atualizado com sucesso.");
        }





        /* Patient Section */
        [HttpGet]
        [PrivilegeUser("SuperAdmim")]
        public IActionResult GetAllPatient()
        {
            return Ok(_userRepository.GetAllUsersGen<PatientModel>());
        }

        [HttpDelete]
        [PrivilegeUser("SuperAdmim")]
        public IActionResult DeletePatient(int id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            try
            {
                var user = _userRepository.GetUserByIdGen<PatientModel>(id);
                if (user == null)
                {
                    return NotFound("Patient not found.");
                }
                string msg = "Delet Patient: " + user.UserId;
                if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
                var result = _userRepository.DeleteUserGen<PatientModel>(user.UserId);
                if (result)
                {

                    return Ok("Patient deleted successfully.");
                }
                return BadRequest("Error deleting patient.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        [PrivilegeUser("SuperAdmim")]
        public IActionResult GetPatientByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }

            var patient = _userRepository.GetUserByEmailGen<PatientModel>(email);
            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            return Ok(patient);
        }

        [HttpGet]
        [UserAcess]
        public IActionResult GetPatientById(int patientId)
        {
            return Ok(_userRepository.GetUserByIdGen<PatientModel>(patientId));
        }

        [PrivilegeUser("Patient")]
        [HttpPost]
        public IActionResult UpdatePatient(PatientModel patient, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }

            try
            {
                var updatedPatient = _userRepository.UpdateUserGen<PatientModel>(patient);
                string msg = "Update Perfil: " + patient.UserId;
                if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
                return Ok(new
                {
                    message = "Patient updated successfully",
                    patient = updatedPatient
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /* metodos aberto sem restrinção*/
        [HttpPost]
        public IActionResult AddPatient(PatientModel patient)
        {
            if (ModelState.IsValid)
            {
                patient.UserType = UserTypeEnum.Patient;
                var addedPatient = _userRepository.AddUserGen(patient);
                return Ok(new
                {
                    message = "Patient added successfully",
                    patient = addedPatient
                });
            }
            else
            {
                return BadRequest("Invalid model state");
            }
        }


        [HttpPost]
        [UserAcess]
        public IActionResult addFile(string fileUrl, [FromHeader(Name = "Authorization")] string authorizationHeader, int? appointId = null)
        {
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loggedUser = _decToken.GetLoggedUser(token);

            if (loggedUser == null)
            {
                return NotFound("Invalid user or password");
            }
            int userId = loggedUser.UserId;
            FileUser image = new FileUser();
            image.ImageUrl = fileUrl;
            image.UserId = userId;

            if (appointId != null)
            {
                bool isSend = _userRepository.IsFileCopy(image, userId, appointId);
                if (isSend)
                {
                    string logMessage = $"User {userId} sent PDF for appointment {appointId.Value}";
                    loggers(logMessage, authorizationHeader);  // Adiciona a linha de log aqui.
                    return Ok("PDF send successfully");
                }
                else
                {
                    return BadRequest("Error sending image");
                }
            }
            else
            {
                bool isSend = _userRepository.IsFileCopy(image, userId);
                if (isSend)
                {
                    string logMessage = $"User {userId} sent an image";
                    loggers(logMessage, authorizationHeader);  // Adiciona a linha de log aqui.
                    return Ok("Image send successfully");
                }
                else
                {
                    return BadRequest("Error sending image");
                }
            }
        }


        //receive token get user logged and return image url
        [HttpGet]
        [UserAcess]
        public IActionResult GetImage([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token");
            }

            var loggedUser = _decToken.GetLoggedUser(token);

            if (loggedUser == null)
            {
                return NotFound("Invalid user or password");
            }

            var imageFile = _userRepository.GetImage();

            if (imageFile != null)
            {
                return Ok(imageFile);
            }
            else
            {
                return BadRequest("Error sending image");
            }
        }


        [HttpGet]

        /*Get Client IP */
        private string GetClientIpAddress(HttpContext context)
        {
            // Verificar CF-Connecting-IP primeiro (específico para Cloudflare)
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

        [HttpDelete]
        [PrivilegeUser("SuperAdmim")]
        [UserAcess]
        public IActionResult SoftDeleteDoctor(int doctorId, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            var user = _userRepository.GetUserByIdGen<DoctorModel>(doctorId);
            string msg = "Soft delete doctor: " + doctorId;
            if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
            if (user == null)
            {
                return NotFound();
            }

            return Ok(_userRepository.SoftDeleteUserGen<DoctorModel>(user.UserId));
        }

        [HttpGet]
        [PrivilegeUser("SuperAdmim")]
        [UserAcess]
        public IActionResult GetAllDeletedDoctors([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            return Ok(_userRepository.GetAllDeletedUsersGen<DoctorModel>());
        }

        [HttpPut]
        [PrivilegeUser("SuperAdmim")]
        [UserAcess]
        public IActionResult RestoreDeletedDoctor(int doctorId, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            string msg = "Restore soft deleted doctor: " + doctorId;
            if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
            return Ok(_userRepository.RestoreDeletedUserGen<DoctorModel>(doctorId));
        }

        [HttpDelete]
        [PrivilegeUser("SuperAdmim")]
        [UserAcess]
        public IActionResult SoftDeletePatient(int patientId, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            var user = _userRepository.GetUserByIdGen<PatientModel>(patientId);
            string msg = "Soft delete patient: " + patientId;
            if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
            if (user == null)
            {
                return NotFound();
            }

            return Ok(_userRepository.SoftDeleteUserGen<PatientModel>(user.UserId));
        }

        [HttpGet]
        [PrivilegeUser("SuperAdmim")]
        [UserAcess]
        public IActionResult GetAllDeletedPatients([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            return Ok(_userRepository.GetAllDeletedUsersGen<PatientModel>());
        }

        [HttpPut]
        [PrivilegeUser("SuperAdmim")]
        [UserAcess]
        public IActionResult RestoreDeletedPatient(int patientId, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            string msg = "Restore soft deleted patient: " + patientId;
            if (!loggers(msg, authorizationHeader)) return BadRequest("Invalid token");
            return Ok(_userRepository.RestoreDeletedUserGen<PatientModel>(patientId));
        }

        [HttpPost]
        public IActionResult RegisterDiseaseStatistic(int userPatientId, int diseaseId, string region)
        {
            try
            {
                bool isRegistered = _userRepository.RegisterRegionDiseaseStatistic(userPatientId, diseaseId, region);

                if (isRegistered)
                {
                    return Ok(new { message = "Disease statistic registered successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to register disease statistic." });
                }
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet]
 
        public IActionResult GetAllDiseasesWithStatistics()
        {
            try
            {
                var desiasewithstatistic = _userRepository.GetAllDiseasesWithStatistics();
                return Ok(desiasewithstatistic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os Disease.");
                return BadRequest("Ocorreu um erro ao buscar os Disease.");
            }
        }

        [HttpGet]

        public IActionResult GetAllDiseases()  
        {
            try
            {
                var desiaseAll = _userRepository.GetAllDiseases();
                return Ok(desiaseAll);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os Disease.");
                return BadRequest("Ocorreu um erro ao buscar os Disease.");
            }
        }


    }


}
