using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; 
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UserAuthApp.Data;
using UserEntity = UserAuthApp.Models.User; 

namespace UserAuthApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthController(ApplicationDbContext context)
        {
            _dbContext = context;
        }


        // ----------- Register -----------
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserEntity model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var sha256 = SHA256.Create();
            var passwordBytes = Encoding.UTF8.GetBytes(model.Password);
            var hash = sha256.ComputeHash(passwordBytes);
            model.PasswordHash = Convert.ToBase64String(hash);

            _dbContext.Users.Add(model);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        // ----------- Login -----------
        public IActionResult Login() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            using var sha256 = SHA256.Create();
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(passwordBytes);
            var passwordHash = Convert.ToBase64String(hash);

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == passwordHash);
            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);
                return RedirectToAction(nameof(Welcome));
            }

            ViewBag.Error = "Geçersiz bilgiler.";
            return View();
        }

        // ----------- Welcome -----------
        public IActionResult Welcome()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            ViewBag.Email = email;
            return View();
        }

        // ----------- Logout -----------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // ----------- Change Password -----------
        public IActionResult ChangePassword()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return NotFound();

            using var sha256 = SHA256.Create();
            var currentHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(currentPassword)));

            if (user.PasswordHash != currentHash)
            {
                ViewBag.Message = "Mevcut şifre yanlış.";
                return View();
            }

            var newHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(newPassword)));
            user.PasswordHash = newHash;

            await _dbContext.SaveChangesAsync();

            ViewBag.Message = "Şifre başarıyla güncellendi.";
            return View();
        }

        // ----------- Profile -----------
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}
