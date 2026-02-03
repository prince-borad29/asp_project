using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;

namespace TaskTracker.Controllers
{
    [Authorize(Roles = "Admin")] // Strict Admin Access
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // 1. List All Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        // 2. Create User (Handled via Modal Form)
        [HttpPost]
        public async Task<IActionResult> Create(string fullName, string email, string password)
        {
            // Basic Validation
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "All fields are required.";
                return RedirectToAction("Index");
            }

            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = "User with this email already exists.";
                return RedirectToAction("Index");
            }

            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "User");

                // ✅ STORE CREDENTIALS FOR THE SUCCESS MODAL
                TempData["NewUser_Name"] = fullName;
                TempData["NewUser_Email"] = email;
                TempData["NewUser_Pass"] = password;

                return RedirectToAction("Index");
            }

            // If failed
            TempData["Error"] = "Error creating user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string fullName, string email)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            // Update details
            user.FullName = fullName;
            user.Email = email;
            user.UserName = email; // Keep username synced with email
            user.NormalizedUserName = email.ToUpper();
            user.NormalizedEmail = email.ToUpper();

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "User updated successfully.";
            }
            else
            {
                TempData["Error"] = "Error updating user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }

        // 4. DELETE USER (POST)
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Prevent deleting yourself (Admin)
                if (User.Identity.Name == user.UserName)
                {
                    TempData["Error"] = "You cannot delete your own account!";
                    return RedirectToAction("Index");
                }

                await _userManager.DeleteAsync(user);
                TempData["Success"] = "User deleted successfully.";
            }
            return RedirectToAction("Index");
        }
    }
}