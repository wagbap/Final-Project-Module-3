using Microsoft.AspNetCore.Mvc;
using LibBiz.Data;
using LibBiz.Models;
using LibBiz.Interfaces;

namespace VendaCursos_MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public LoginController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            // Definir a senha como UserID antes da criação
            user.Password = user.Email.ToString(); // Supondo que UserID é um int.

            var createdUser = await _userRepository.Create(user);

            if (createdUser != null)
            {
                // Redirecionar para a página de login após o registro bem-sucedido.
                return RedirectToAction("Index", "Login"); // Assumindo que você tem um método 'Login' no 'LoginController'.
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Erro ao criar usuário.");
                return View(user);
            }
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
