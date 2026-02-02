using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Models;

namespace TaskTracker.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admin can access
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // 1. List all users
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // 2. Create User (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. Create User (POST)
        [HttpPost]
        public async Task<IActionResult> Create(string fullName, string email, string password)
        {
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "User");

                // Pass data to View to show the "Copy/Share" modal
                TempData["CreatedUser"] = fullName;
                TempData["CreatedEmail"] = email;
                TempData["CreatedPass"] = password;

                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
    }
}