using HRS_2.Models.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRS_2._0.Repositories.Abstract;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using HRS_2.Models;

namespace HRS_2._0.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IActiveDirectoryService _adService;

        public AccountController(DatabaseContext context, IActiveDirectoryService adService)
        {
            _context = context;
            _adService = adService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(string username, string password)
        {
            // First try AD authentication
            if (_adService.IsAuthenticated("localnet.mynet", username, password, out var errorMessage))
            {
                var adUser = new User { Username = username, Roles = "User", Nama = username };
                SignInUser(adUser);
                return RedirectToAction("Index", "Home");
            }

            // If AD auth fails, try database auth
            var user = _context.User.SingleOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                SignInUser(user);
                return RedirectToAction("Index", "Home");
            }

            // Both authentication methods failed
            ViewBag.ErrorMessage = errorMessage ?? "Nama pengguna atau kata laluan tidak sah.";
            return View();
        }

        public IActionResult Homepage()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        private void SignInUser(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.Nama ?? user.Username),
                new Claim(ClaimTypes.Role, user.Roles ?? "User")
            };
            var identity = new ClaimsIdentity(claims, "Custom");
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(principal);

            // Set session variables
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.Nama ?? user.Username);
            HttpContext.Session.SetString("UserRole", user.Roles ?? "User");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}