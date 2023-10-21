using eConsultas_MVC.Models;
using eConsultas_MVC.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using static eConsultas_MVC.Models.DiseaseVM;

namespace eConsultas_MVC.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ImgToDir _imgToDir;

        public DashBoardController(ImgToDir imgToDir, IHttpClientFactory httpClientFactory)
        {
            _imgToDir = imgToDir;
            _httpClientFactory = httpClientFactory;
            // Use the factory to create a HttpClient instance
            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5242/");
        }


        /* Login */
        public IActionResult Index()
        {
            string _token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(_token))
            {
                return View();
            }

            return RedirectToAction("DashLand");
        }

        /* Dashboard */
        public async Task<IActionResult> DashLand()
        {
            string userImg = null;
            await Task.Delay(TimeSpan.FromSeconds(1)); // Atraso de 1 segundos

            string _token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(_token))
            {
                return RedirectToAction("Index");
            }
            else
            {
                var user = await GetUser(_token);
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));

                var getUser = JsonConvert.DeserializeObject<UserMV>(HttpContext.Session.GetString("User"));
                var img = GetAllUserFiles("/img/").Result;
                userImg = img.FirstOrDefault(x => x.UserId == getUser.UserId)?.ImageUrl;
                if (!string.IsNullOrEmpty(userImg))
                {
                    HttpContext.Session.SetString("UserImg", userImg);
                }

                var viewModel = new DashboardMV
                {
                    Appointments = await GetAppointments(_token)
                };

                return View(viewModel);
            }
        }




        /* Get user Logged Info */
        private async Task<UserMV> GetUser(string token)
        {
            string apiUrl = "Login/GetUserLogged";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    UserMV user = JsonConvert.DeserializeObject<UserMV>(responseContent);

                    return user;
                }
                else
                {
                    return new UserMV();
                }
            }
            catch (Exception ex)
            {
                return new UserMV();
            }
        }





        /* view create appointment*/
        [HttpGet]
        public async Task<IActionResult> CreateAppoint(string region, string city, string specialization)
        {
            string token = HttpContext.Session.GetString("Token");
            List<DoctorMV> doctors = await GetDoctorsBy(token, region, city, specialization);

            return View(doctors);
        }

        public IActionResult SendAppointment(int id)
        {
            //get doctor by id
            string token = HttpContext.Session.GetString("Token");
            var doctor = GetDoctors(token).Result.FirstOrDefault(x => x.UserId == id);

            return View(doctor);
        }

        /* Create Appointment just Patient can create a new appointment by add doctor and message*/
        [HttpPost]
        public async Task<IActionResult> AddApointment(int doctorId, string patientMessage)
        {
            string token = HttpContext.Session.GetString("Token");
            var user = JsonConvert.DeserializeObject<UserMV>(HttpContext.Session.GetString("User"));

            // Build the query parameters as a dictionary
            var queryParams = new Dictionary<string, string>
            {
                { "doctorId", doctorId.ToString() },
                { "patientMessage", patientMessage }
            };

            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

            string apiUrl = $"Appointments/CreateAppointment?{queryString}";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("DashLand");
                }
                else
                {
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Erro");
            }
        }

        /* Get all appointments by user id */
        private async Task<List<AppointmentMV>> GetAppointments(string token)
        {
            string apiUrl = "Appointments/GetAppointById";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var genericModels = JsonConvert.DeserializeObject<List<AppointmentMV>>(responseContent);
                    return genericModels;
                }
                else
                {
                    return new List<AppointmentMV>(null);
                }
            }
            catch (Exception ex)
            {
                return new List<AppointmentMV>(null);
            }
        }

        //finish appointment
        public async Task<IActionResult> FinishAppointment(int id)
        {
            string token = HttpContext.Session.GetString("Token");


            var queryParams = new Dictionary<string, string>
            {
                { "appointmentId", id.ToString() }
            };


            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;


            string apiUrl = $"Appointments/FinishAppointment?{queryString}";

            try
            {

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("DashLand");
                }
                else
                {
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                return RedirectToAction("Erro");
            }
        }

        /* view Patient */
        public IActionResult CreatePatient()
        {
            return View();
        }

        /* Create Patient */
        [HttpPost]
        public IActionResult CreatePatient(CreatePatientMV createPatient)
        {
            string token = HttpContext.Session.GetString("Token");

            var content = new StringContent(JsonConvert.SerializeObject(createPatient), Encoding.UTF8, "application/json");

            HttpResponseMessage response = _httpClient.PostAsync("Users/Addpatient", content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("DashLand");
            }
            else
            {
                return RedirectToAction("Erro");
            }
        }


        /* view docotr */
        public IActionResult CreateDoctor()
        {
            return View();
        }


        /* Create Patient */
        [HttpPost]
        public IActionResult CreateDoctor(CreatePatientMV createPatient)
        {
            string token = HttpContext.Session.GetString("Token");

            var content = new StringContent(JsonConvert.SerializeObject(createPatient), Encoding.UTF8, "application/json");

            HttpResponseMessage response = _httpClient.PostAsync("Users/AddDoctor", content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CreateDoctor");
            }
            else
            {
                return RedirectToAction("Erro");
            }
        }


        /* view docotr */
        public IActionResult Chart()
        {
            var viewModel = new DiseaseVM();
            // Preencha viewModel.DiseasesStatistics aqui...

            return View(viewModel); // Certifique-se de que não está retornando null.
        }


        // GET method DiseaseStatistic
        public async Task<IActionResult> CreateDiseaseStatistic()
        {
            var model = new DiseaseVM();

            var patients = await GetAllPatientsDesiase();
            var diseases = await GetAllDisease();
            var diseasesWithStats = await GetAllDiseaseStatistic(); // Ensure this method fetches data from the correct endpoint

            model.Patients = patients.Select(p => new SelectListItem(p.FullName, p.UserId.ToString())).ToList();
            model.Regions = patients.Select(p => new SelectListItem(p.Region, p.Region)).Distinct().ToList();
            model.Diseases = diseases.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();


            model.DiseasesStatistics = diseasesWithStats.Select(d => new DiseaseVM.DiseaseStatisticMV
            {
                Id = d.Id,
                UserId = d.UserId,
                DiseaseName = d.DiseaseName,
                Region = d.Region,
                DeathStatus = d.DeathStatus,
                FullName = d.FullName
            }).ToList();

            return View(model);
        }



        public async Task<List<DiseaseVM>> GetAllDisease()
        {
            string token = HttpContext.Session.GetString("Token");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage diseaseResponse = await _httpClient.GetAsync("http://localhost:5242/Users/GetAllDiseases");

            if (diseaseResponse.IsSuccessStatusCode)
            {
                var responseContent = await diseaseResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DiseaseVM>>(responseContent);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter doenças: {diseaseResponse.StatusCode.ToString()}");
                return new List<DiseaseVM>();
            }
        }




        public async Task<List<DiseaseStatisticMV>> GetAllDiseaseStatistic()
        {
            string token = HttpContext.Session.GetString("Token");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage diseaseResponse = await _httpClient.GetAsync("http://localhost:5242/Users/GetAllDiseasesWithStatistics");

            if (diseaseResponse.IsSuccessStatusCode)
            {
                var responseContent = await diseaseResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DiseaseStatisticMV>>(responseContent);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter doenças: {diseaseResponse.StatusCode.ToString()}");
                return new List<DiseaseStatisticMV>();
            }
        }


        public async Task<List<PatientMV>> GetAllPatientsDesiase()
        {
            string token = HttpContext.Session.GetString("Token");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage patientResponse = await _httpClient.GetAsync("http://localhost:5242/Users/GetAllPatient");

            if (patientResponse.IsSuccessStatusCode)
            {
                var responseContent = await patientResponse.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(responseContent);

                var patients = JsonConvert.DeserializeObject<List<PatientMV>>(responseContent);

                System.Diagnostics.Debug.WriteLine(patients.Count);

                return patients;
            }
            else
            {

                System.Diagnostics.Debug.WriteLine($"Erro ao obter pacientes: {patientResponse.StatusCode.ToString()}");

                return new List<PatientMV>();
            }
        }

        public async Task<(List<PatientMV>, List<string>)> GetAllPatientsAndRegions()
        {
            string token = HttpContext.Session.GetString("Token");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage patientResponse = await _httpClient.GetAsync("http://localhost:5242/Users/GetAllPatient");

            if (patientResponse.IsSuccessStatusCode)
            {
                var responseContent = await patientResponse.Content.ReadAsStringAsync();
                var patients = JsonConvert.DeserializeObject<List<PatientMV>>(responseContent);


                var regions = patients.Select(p => p.Region).Distinct().ToList();

                return (patients, regions);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter pacientes: {patientResponse.StatusCode.ToString()}");
                return (new List<PatientMV>(), new List<string>());
            }
        }





        // POST method
        [HttpPost]

        public async Task<IActionResult> CreateDiseaseStatistic(int? userPatientId, int? diseaseId, string region)
        {
            if (!ModelState.IsValid || !userPatientId.HasValue || !diseaseId.HasValue || string.IsNullOrWhiteSpace(region))
            {
                var model = new DiseaseVM();
                ModelState.AddModelError(string.Empty, "Invalid input data.");
                return View(model);
            }

            var queryParams = new Dictionary<string, string>
            {
                { "userPatientId", userPatientId.ToString() },
                { "diseaseId", diseaseId.ToString() },
                { "region", region }
            };

            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;
            string apiUrl = $"http://localhost:5242/Users/RegisterDiseaseStatistic?{queryString}";

            var response = await _httpClient.PostAsync(apiUrl, null);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CreateDiseaseStatistic");
            }
            else
            {

                var responseContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Failed to register disease statistic. Status code: {response.StatusCode}. Response: {responseContent}");

                var model = new DiseaseVM();
                return View(model);
            }
        }

        /* view Chat */
        [HttpGet]
        public async Task<IActionResult> Message(int id)
        {
            if (id != null)
            {
                string token = HttpContext.Session.GetString("Token");
                var appointment = GetAppointments(token).Result.FirstOrDefault(x => x.AppointId == id);

                if (appointment == null)
                {
                    return NotFound(null);
                }

                var messages = await GetMessages(token, id.ToString());
                var viewMessageModel = new MessageListsMV
                {
                    Messages = messages,
                    Appointments = appointment,
                    FilesImg = GetAllUserFiles("/img/").Result,
                    FilesPdf = GetAllUserFiles("/pdf/").Result
                };

                return View(viewMessageModel);
            }
            else
            {
                return null;
            }
        }

        /*Get all message by appointment Id*/
        public async Task<List<MessageMV>> GetMessages(string token, string appointmentId)
        {
            string apiUrl = $"Appointments/GetMessageByAppointId?appointmentId={appointmentId}";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<MessageMV> messages = JsonConvert.DeserializeObject<List<MessageMV>>(responseContent);
                    return messages;
                }
                else
                {
                    return new List<MessageMV>(null);
                }
            }
            catch (Exception ex)
            {
                return new List<MessageMV>(null);
            }
        }


        /* Add message to appointment */
        [HttpPost]
        public async Task<IActionResult> AddMessage(string appointmentId, string message)
        {
            string token = HttpContext.Session.GetString("Token");
            var user = JsonConvert.DeserializeObject<UserMV>(HttpContext.Session.GetString("User"));

            var queryParams = new Dictionary<string, string>
            {
                { "appointmentId", appointmentId },
                { "message", message }
            };

            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

            string apiUrl = $"Appointments/AddMessage?{queryString}";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Message", "DashBoard", new { id = appointmentId });
                }
                else
                {
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Erro");
            }
        }

        /* Login */
        public async Task<IActionResult> Login(LoginMV login)
        {
            if (!ModelState.IsValid) return View(login);

            string apiUrl = "Login/Login";

            var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Token");

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    HttpContext.Session.SetString("Token", responseContent);
                    return RedirectToAction("DashLand");
                }
                else
                {
                    return RedirectToAction("Index");
                }

            }
            else
            {
                return RedirectToAction("Erro");
            }
        }





        /* view updates users info*/
        public IActionResult UpdateUser()
        {
            string token = HttpContext.Session.GetString("Token");
            var user = JsonConvert.DeserializeObject<UserMV>(HttpContext.Session.GetString("User"));

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index");
            }

            var viewModel = new UsersInfo { User = user };

            switch (user.UserType)
            {
                case "Doctor":
                    viewModel.Doctor = GetDoctors(token).Result.FirstOrDefault(x => x.UserId == user.UserId);
                    break;

                case "Patient":

                    break;

                case "SuperAdmin":

                    break;

                default:
                    return RedirectToAction("Index");
            }

            return View(viewModel);
        }



        //update user Info
        [HttpPost]
        public IActionResult UpdateUser(DoctorMV doctor)
        {

            string token = HttpContext.Session.GetString("Token");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(doctor), Encoding.UTF8, "application/json");

            HttpResponseMessage response = _httpClient.PostAsync("Users/UpdateDoctor", content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("UpdateUser");
            }
            else
            {
                return RedirectToAction("Erro");
            }
        }


        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int? doctorId, int? patientId, int status)
        {
            // Check if the token is valid. If not, redirect to the login page.
            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index");
            }


            var queryParams = new Dictionary<string, string>();

            if (doctorId.HasValue)
            {
                queryParams.Add("doctorUserId", doctorId.Value.ToString());
            }

            if (patientId.HasValue)
            {
                queryParams.Add("patientUserId", patientId.Value.ToString());
            }

            queryParams.Add("status", status.ToString());


            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;


            string apiUrl = $"http://localhost:5242/Users/UpdateUserStatus?{queryString}";

            try
            {

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    if (doctorId.HasValue)
                    {
                        return RedirectToAction("GetAllDoctors");
                    }
                    else if (patientId.HasValue)
                    {
                        return RedirectToAction("GetAllPatients");
                    }
                    else
                    {
                        return RedirectToAction("Erro");
                    }
                }
                else
                {
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {

                return RedirectToAction("Erro");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDeaphStatus(int? userId, bool deathStatus)
        {

            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index");
            }


            var queryParams = new Dictionary<string, string>();


            if (userId.HasValue)
            {
                queryParams.Add("patientUserId", userId.Value.ToString());
            }

            queryParams.Add("status", deathStatus.ToString());


            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

            // Construct the full API URL with the query string. 
            string apiUrl = $"http://localhost:5242/Users/UpdateDeaphStatus?{queryString}";

            try
            {

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    if (userId.HasValue)
                    {
                        return RedirectToAction("CreateDiseaseStatistic");
                    }
                    else
                    {
                        return RedirectToAction("Erro");
                    }
                }
                else
                {
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {

                return RedirectToAction("Erro");
            }
        }







        //change password
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordMV changePassword)
        {
            string token = HttpContext.Session.GetString("Token");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(changePassword), Encoding.UTF8, "application/json");

            HttpResponseMessage response = _httpClient.PostAsync("Login/ChangePwd", content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("UpdateUser");
            }
            else
            {
                return RedirectToAction("Erro");
            }
        }

        // save user image/pdf 
        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile file, int? appointId = null)
        {
            if (file == null)
            {
                return RedirectToAction("Erro");
            }

            string token = HttpContext.Session.GetString("Token");
            string fileUrl = null;

            try
            {
                List<string> imageExtensions = new List<string> { ".jpeg", ".png", ".jpg" };
                List<string> pdfExtensions = new List<string> { ".pdf" };

                string uploadDirectory = null;

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (imageExtensions.Contains(extension))
                {
                    uploadDirectory = "img/Upload";
                }
                else if (pdfExtensions.Contains(extension))
                {
                    uploadDirectory = "pdf/Upload";
                }
                else
                {
                    return RedirectToAction("Erro");
                }

                fileName = fileName + "_" + DateTime.Now.ToString("yymmfff") + extension;
                var filePath = Path.Combine("wwwroot", uploadDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                fileUrl = filePath;

                if (fileUrl == null)
                {
                    return RedirectToAction("Erro");
                }

                var content = new StringContent($"fileUrl={fileUrl}", Encoding.UTF8, "application/x-www-form-urlencoded");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                HttpResponseMessage response;

                if (appointId == null)
                {
                    response = await _httpClient.PostAsync($"Users/addFile?fileUrl={fileUrl}", content);
                }
                else
                {
                    response = await _httpClient.PostAsync($"Users/addFile?fileUrl={fileUrl}&appointId={appointId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("DashLand");
                }
                else
                {
                    return RedirectToAction("Erro");
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("Erro");
            }
        }


        public async Task<IActionResult> DownloadPdf(int appointId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"Appointments/GetPdfByAppointment?appointId={appointId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fileResponse = JsonConvert.DeserializeObject<FileUserMV>(content);
                var path = fileResponse.ImageUrl.Trim();

                var memory = new MemoryStream();

                try
                {
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }
                    memory.Position = 0;

                    // forçar o download
                    return File(memory, "application/pdf", Path.GetFileName(path));
                }
                catch (FileNotFoundException)
                {
                    return NotFound("PDF file has been deleted or does not exist.");
                }
                catch (Exception ex)
                {
                    return BadRequest($"An error occurred: {ex.Message}");
                }
            }

            return RedirectToAction("Erro");
        }




        //get user logged image/pdf
        public async Task<List<FileMV>> GetAllUserFiles(string directory)
        {
            string token = HttpContext.Session.GetString("Token");
            string apiUrl = $"Users/GetImage";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<FileMV> files = JsonConvert.DeserializeObject<List<FileMV>>(responseContent);
                    List<FileMV> returnFiles = new List<FileMV>();
                    files.ForEach(x =>
                    {

                        if (x.ImageUrl.Contains("img"))
                        {
                            x.ImageUrl = x.ImageUrl.Replace("wwwroot\\", "~/");
                            x.ImageUrl = x.ImageUrl.Replace("\\", "/");

                            returnFiles.Add(x);
                        }
                    });


                    return returnFiles;
                }
                else
                {
                    return new List<FileMV>();
                }
            }
            catch (Exception ex)
            {
                return new List<FileMV>();
            }
        }

        public IActionResult Erro()
        {
            return View();
        }


        public async Task<IActionResult> GetAllLogs(DateTime? startDate = null, DateTime? endDate = null, string userId = null)
        {
            string _token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(_token))
            {
                return RedirectToAction("Index");
            }

            List<LogModel> logs;
            if (startDate.HasValue || endDate.HasValue || !string.IsNullOrEmpty(userId))
            {
                logs = await FetchLogsByDateRangeFromApi(_token, startDate, endDate, userId);
            }
            else if (TempData["FilteredLogs"] != null)
            {
                logs = JsonConvert.DeserializeObject<List<LogModel>>(TempData["FilteredLogs"].ToString());
            }
            else
            {
                logs = await FetchAllLogsFromApi(_token);
            }

            var recentLogs = await FetchLatestEighLogsFromApi(_token);
            var viewModel = new LogsViewModel
            {
                AllLogs = logs,
                RecentLogs = recentLogs
            };
            return View(viewModel);
        }



        private async Task<List<LogModel>> FetchAllLogsFromApi(string token)
        {
            string apiUrl = "http://localhost:5242/Users/GetAllLogs";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<LogModel> logs = JsonConvert.DeserializeObject<List<LogModel>>(responseContent);
                    return logs;
                }
                else
                {
                    return new List<LogModel>();
                }
            }
            catch (Exception ex)
            {
                return new List<LogModel>();
            }
        }



        public async Task<IActionResult> GetLatestEightLogs()
        {
            string _token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(_token))
            {
                return RedirectToAction("Index");
            }
            var allLogs = await FetchAllLogsFromApi(_token);
            var recentLogs = await FetchLatestEighLogsFromApi(_token);
            var viewModel = new LogsViewModel
            {
                AllLogs = allLogs,
                RecentLogs = recentLogs
            };
            return View(viewModel);
        }

        private async Task<List<LogModel>> FetchLatestEighLogsFromApi(string token)
        {
            string apiUrl = "http://localhost:5242/Users/GetLatestEightLogs";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<LogModel> logs = JsonConvert.DeserializeObject<List<LogModel>>(responseContent);
                    return logs;
                }
                else
                {
                    return new List<LogModel>();
                }
            }
            catch (Exception ex)
            {
                return new List<LogModel>();
            }
        }




        public async Task<IActionResult> GetLogsByDateRanges(DateTime? startDate, DateTime? endDate, string userId = null)
        {
            string _token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(_token))
            {
                return RedirectToAction("Index");
            }

            // Puxe os logs filtrados por data.
            var logsByDateRange = await FetchLogsByDateRangeFromApi(_token, startDate, endDate, userId);


            TempData["FilteredLogs"] = JsonConvert.SerializeObject(logsByDateRange);

            return RedirectToAction("GetAllLogs");
        }


        private async Task<List<LogModel>> FetchLogsByDateRangeFromApi(string token, DateTime? startDate, DateTime? endDate, string userId = null)
        {
            var queryParams = new Dictionary<string, string>();
            if (startDate.HasValue)
                queryParams.Add("startDate", startDate.Value.ToString("yyyy-MM-dd"));
            if (endDate.HasValue)
                queryParams.Add("endDate", endDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(userId))
                queryParams.Add("userId", userId);

            var queryString = await new FormUrlEncodedContent(queryParams).ReadAsStringAsync();
            var apiUrl = $"http://localhost:5242/Users/GetLogsByDateRange?{queryString}";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);  // Log para depuração
                    List<LogModel> logs = JsonConvert.DeserializeObject<List<LogModel>>(responseContent);
                    return logs;
                }
                else
                {
                    Console.WriteLine($"Erro: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");  // Log para depuração
                    return new List<LogModel>();
                }

            }
            catch (Exception ex)
            {
                return new List<LogModel>();
            }
        }





        //get all doctors
        public async Task<List<DoctorMV>> GetDoctors(string token)
        {
            string apiUrl = "Users/GetAllDoctor";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var genericModels = JsonConvert.DeserializeObject<List<DoctorMV>>(responseContent);
                    return genericModels;
                }
                else
                {
                    return new List<DoctorMV>(null);
                }
            }
            catch (Exception ex)
            {
                return new List<DoctorMV>(null);
            }
        }



        public async Task<IActionResult> GetAllDoctors()
        {
            string _token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(_token))
            {

                return RedirectToAction("Index");
            }

            var doctors = await FetchAllDoctorsFromApi(_token);
            return View("GetAllDoctors", doctors);
        }

        private async Task<List<DoctorMV>> FetchAllDoctorsFromApi(string token)
        {
            string apiUrl = "http://localhost:5242/Users/GetAllDoctor";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<DoctorMV> doctors = JsonConvert.DeserializeObject<List<DoctorMV>>(responseContent);
                    return doctors;
                }
                else
                {
                    return new List<DoctorMV>();
                }
            }
            catch (Exception ex)
            {

                return new List<DoctorMV>();
            }
        }

        //get doctor by 
        public async Task<List<DoctorMV>> GetDoctorsBy(string token, string? region = null, string? city = null, string? specialization = null)
        {
            try
            {
                string apiUrl = "http://localhost:5242/Users/DoctorBy";

                var queryParameters = new List<string>();
                if (!string.IsNullOrEmpty(region))
                {
                    queryParameters.Add($"region={region}");
                }
                if (!string.IsNullOrEmpty(city))
                {
                    queryParameters.Add($"city={city}");
                }
                if (!string.IsNullOrEmpty(specialization))
                {
                    queryParameters.Add($"specialization={specialization}");
                }

                if (queryParameters.Count > 0)
                {
                    apiUrl += "?" + string.Join("&", queryParameters);
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<DoctorMV> doctors = JsonConvert.DeserializeObject<List<DoctorMV>>(responseContent);
                    return doctors;
                }
                else
                {
                    return new List<DoctorMV>(null);
                }
            }
            catch (Exception ex)
            {
                return new List<DoctorMV>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeletedDoctors()
        {
            string token = HttpContext.Session.GetString("Token");


            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index");
            }


            string apiUrl = "http://localhost:5242/Users/GetAllDeletedDoctors";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {

                    var deleteddoctors = JsonConvert.DeserializeObject<List<DoctorMV>>(await response.Content.ReadAsStringAsync());
                    return View(deleteddoctors);  // Retorne a lista para a view.
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error getting deleted doctors: {errorMessage}");
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting deleted doctors: {ex.Message}");
                return RedirectToAction("Erro");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RestoreDeletedDoctor(int doctorId)
        {
            string token = HttpContext.Session.GetString("Token");


            var queryParams = new Dictionary<string, string>
            {
                { "doctorId", doctorId.ToString() }
            };

            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;


            string apiUrl = $"http://localhost:5242/Users/RestoreDeletedDoctor?{queryString}";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetAllDoctors");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error restoring doctor: {errorMessage}");
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception restoring doctor: {ex.Message}");
                return RedirectToAction("Erro");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RestoreDeletedPatient(int patientId)
        {
            string token = HttpContext.Session.GetString("Token");


            var queryParams = new Dictionary<string, string>
            {
          
                {
                    "patientId", patientId.ToString()
                }
            };

            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

           
            string apiUrl = $"http://localhost:5242/Users/RestoreDeletedPatient?{queryString}";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

               
                HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetAllPatients"); 
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error restoring patient: {errorMessage}");
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception restoring patient: {ex.Message}");
                return RedirectToAction("Erro");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllDeletedPatients()
        {
            string token = HttpContext.Session.GetString("Token");

            
            string apiUrl = "http://localhost:5242/Users/GetAllDeletedPatients";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                   
                    var deletedPatients = JsonConvert.DeserializeObject<List<PatientMV>>(await response.Content.ReadAsStringAsync());
                    return View(deletedPatients);  // Retorne a lista para a view.
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error getting deleted patients: {errorMessage}");
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting deleted patients: {ex.Message}");
                return RedirectToAction("Erro");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeletePatients(int patientId)
        {
            string token = HttpContext.Session.GetString("Token");
            var user = JsonConvert.DeserializeObject<UserMV>(HttpContext.Session.GetString("User"));

           
            var queryParams = new Dictionary<string, string>
            {
                { "patientId", patientId.ToString() }
            };

            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

            string apiUrl = $"http://localhost:5242/Users/SoftDeletePatient?{queryString}";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetAllPatients");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deleting patient: {errorMessage}");
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting patient: {ex.Message}");
                return RedirectToAction("Erro");
            }
        }





        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(int doctorId)
        {
            string token = HttpContext.Session.GetString("Token");
            var user = JsonConvert.DeserializeObject<UserMV>(HttpContext.Session.GetString("User"));

           
            var queryParams = new Dictionary<string, string>
            {
                { "doctorId", doctorId.ToString() }
            };

            var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

            string apiUrl = $"http://localhost:5242/Users/SoftDeleteDoctor?{queryString}";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetAllDoctors");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deleting doctor: {errorMessage}");
                    return RedirectToAction("Erro");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting doctor: {ex.Message}");
                return RedirectToAction("Erro");
            }
        }





        public async Task<IActionResult> GetAllPatients()
        {
            string _token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(_token))
            {
               
                return RedirectToAction("Index");
            }

            var patients = await FetchAllPatientsFromApi(_token);
            return View("GetAllPatients", patients);  


        }


        private async Task<List<PatientMV>> FetchAllPatientsFromApi(string token)
        {
            string apiUrl = "http://localhost:5242/Users/GetAllPatient";  

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<PatientMV> patients = JsonConvert.DeserializeObject<List<PatientMV>>(responseContent);
                    return patients;
                }
                else
                {
                    return new List<PatientMV>();
                }
            }
            catch (Exception ex)
            {
                
                return new List<PatientMV>();
            }
        }

        private async Task<DoctorMV> GetDoctorById(int doctorId)
        {
            
            return new DoctorMV();  
        }

        //logout clear session
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Token");
            return RedirectToAction("Index", "Home");
        }
    }
}
