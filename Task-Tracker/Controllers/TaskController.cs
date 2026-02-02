using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;
using TaskTracker.Models.ViewModels;

namespace TaskTracker.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env; // For saving files

        public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: Create Task Page
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new CreateTaskViewModel
            {
                // Load all users so Admin can select them
                AllUsers = _userManager.Users.ToList()
            };
            return View(model);
        }

        // POST: Save Task
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Create the Task Object
                var task = new AppTask
                {
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    Priority = model.Priority,
                    Status = AppTaskStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                // 2. Handle File Upload (If exists)
                if (model.Attachment != null)
                {
                    string folder = Path.Combine(_env.WebRootPath, "attachments");
                    Directory.CreateDirectory(folder); // Ensure folder exists
                    string fileName = Guid.NewGuid().ToString() + "_" + model.Attachment.FileName;
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }
                    task.AttachmentPath = fileName;
                }

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync(); // Save to get the new Task ID

                // 3. Save Checklist Items
                if (model.ChecklistItems != null && model.ChecklistItems.Any())
                {
                    foreach (var itemDesc in model.ChecklistItems)
                    {
                        if (!string.IsNullOrWhiteSpace(itemDesc))
                        {
                            _context.TodoItems.Add(new TodoItem { Description = itemDesc, AppTaskId = task.Id });
                        }
                    }
                }

                // 4. Save Assigned Users
                if (model.SelectedUserIds != null)
                {
                    foreach (var userId in model.SelectedUserIds)
                    {
                        _context.TaskAssignments.Add(new TaskAssignment
                        {
                            AppTaskId = task.Id,
                            ApplicationUserId = userId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            // If failed, reload users
            model.AllUsers = _userManager.Users.ToList();
            return View(model);
        }
    }
}