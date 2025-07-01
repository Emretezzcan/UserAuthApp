using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UserAuthApp.Data;
using UserAuthApp.Models;

namespace UserAuthApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(user.PasswordHash);
                var hash = sha256.ComputeHash(bytes);
                user.PasswordHash = Convert.ToBase64String(hash);

                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            string passwordHash = Convert.ToBase64String(hash);

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == passwordHash);
            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);
                return RedirectToAction("Welcome","Auth");
            }

            ViewBag.Error = "Geçersiz bilgiler.";
            return View();
        }

        public IActionResult Welcome()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

