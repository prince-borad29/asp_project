using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;
using TaskTracker.Models.ViewModels;

namespace TaskTracker.Controllers
{
    [Authorize] // Must be logged in
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            IQueryable<AppTask> taskQuery = _context.Tasks.Include(t => t.Assignments);

            // FILTER: If not Admin, only show my tasks
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                taskQuery = taskQuery.Where(t => t.Assignments.Any(a => a.ApplicationUserId == user.Id));
            }

            var tasks = await taskQuery.ToListAsync();

            // PREPARE DATA
            var model = new DashboardViewModel
            {
                TotalTasks = tasks.Count,
                PendingTasks = tasks.Count(t => t.Status == AppTaskStatus.Pending),
                InProgressTasks = tasks.Count(t => t.Status == AppTaskStatus.InProgress),
                CompletedTasks = tasks.Count(t => t.Status == AppTaskStatus.Completed),

                // Data for Pie Chart [Pending, InProgress, Completed]
                StatusCounts = new int[]
                {
                    tasks.Count(t => t.Status == AppTaskStatus.Pending),
                    tasks.Count(t => t.Status == AppTaskStatus.InProgress),
                    tasks.Count(t => t.Status == AppTaskStatus.Completed)
                },

                // Data for Bar Chart [Low, Medium, High]
                PriorityCounts = new int[]
                {
                    tasks.Count(t => t.Priority == TaskPriority.Low),
                    tasks.Count(t => t.Priority == TaskPriority.Medium),
                    tasks.Count(t => t.Priority == TaskPriority.High)
                },

                // Get 3 most recent tasks
                RecentTasks = tasks.OrderByDescending(t => t.CreatedAt).Take(3).ToList()
            };

            return View(model);
        }
    }
}