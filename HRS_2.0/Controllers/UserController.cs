using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Mail;
using System.Net;
using HRS_2.Models.Domain;
using HRS_2.Models;

namespace TeKarSistem.Controllers
{
    public class UserController : Controller
    {
    
        private readonly DatabaseContext _context;

        private readonly ILogger<UserController> _logger;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        public UserController(
            DatabaseContext context,

            ILogger<UserController> logger,
            IConfiguration configuration)
        {
            _context = context;

            _logger = logger;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
        }

        // Display users in Index view
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _context.User?.ToListAsync() ?? new List<User>();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users");
                TempData["ErrorMessage"] = "An error occurred while retrieving users. Please try again later.";
                return View(new List<User>());
            }
        }

        public async Task<IActionResult> ToggleWindowsAuth(string noPekerja)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.NoPekerja == noPekerja);
                if (user != null)
                {
                    user.UseWindowsAuth = !user.UseWindowsAuth;
                    if (user.UseWindowsAuth)
                    {
                        user.Password = null; // If Windows Auth is enabled, set Password to NULL
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Windows authentication {(user.UseWindowsAuth ? "enabled" : "disabled")} for {user.Nama}";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling Windows authentication");
                TempData["ErrorMessage"] = "An error occurred while changing authentication settings.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username)
        {
            var userExists = await _context.User.AnyAsync(u => u.Username == username);
            return Json(new { isAvailable = !userExists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckNoPekerja(string noPekerja)
        {
            var employeeExists = await _context.User.AnyAsync(u => u.NoPekerja == noPekerja);
            return Json(new { isAvailable = !employeeExists });
        }



        public IActionResult Create()
        {
            return View(new User { BlockUser = false, Roles = "User" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                // Check Username
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "This username is already taken. Please choose another one.");
                    return View(user);
                }

                // Check NoPekerja
                var existingEmployee = await _context.User.FirstOrDefaultAsync(u => u.NoPekerja == user.NoPekerja);
                if (existingEmployee != null)
                {
                    ModelState.AddModelError("NoPekerja", "This NoPekerja is already in use.");
                    return View(user);
                }

                // Proceed with saving user
                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string noPekerja)
        {
            if (string.IsNullOrWhiteSpace(noPekerja))
            {
                TempData["ErrorMessage"] = "No staff number provided.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.User
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.NoPekerja == noPekerja);

            if (user == null)
            {
                TempData["ErrorMessage"] = $"User with staff number {noPekerja} not found.";
                return RedirectToAction(nameof(Index));
            }

            // Additional security: mask or remove sensitive password information
            user.Password = null;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string noPekerja, User user)
        {
            if (noPekerja != user.NoPekerja)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.User
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.NoPekerja == noPekerja);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Handle password scenarios
                    if (string.IsNullOrEmpty(user.Password))
                    {
                        // If password field is empty, keep the existing password
                        user.Password = existingUser.Password;
                    }
                    else
                    {
                        // If password is provided and different from existing, you might want to hash it
                        if (user.Password != existingUser.Password)
                        {
                            // Hash password or process it as needed
                            // user.Password = HashPassword(user.Password);
                        }
                    }

                    // Clear password if using Windows Authentication
                    if (user.UseWindowsAuth)
                    {
                        user.Password = null;
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.NoPekerja))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(user);
        }

        // POST: User/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string noPekerja)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'DatabaseContext.User' is null.");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.NoPekerja == noPekerja);
            if (user != null)
            {
                try
                {
                    _context.User.Remove(user);
                    await _context.SaveChangesAsync();

                    // Add a TempData message for success notification
                    TempData["SuccessMessage"] = $"Pengguna {user.Username} berjaya dipadam.";

                    _logger.LogInformation($"User with NoPekerja {noPekerja} deleted successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting User with NoPekerja {noPekerja}");
                    TempData["ErrorMessage"] = "Ralat berlaku semasa memadam pengguna.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Pengguna tidak dijumpai.";
            }

            return RedirectToAction(nameof(Index));
        }


        private bool UserExists(string noPekerja)
        {
            return _context.User.Any(e => e.NoPekerja == noPekerja);
        }
    }
}
