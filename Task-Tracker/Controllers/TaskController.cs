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

        // 1. GET: List All Tasks
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.ApplicationUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tasks);
        }

        // 2. POST: Delete Task
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                // Remove file if exists
                if (!string.IsNullOrEmpty(task.AttachmentPath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, "attachments", task.AttachmentPath);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. EDIT TASK 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.TodoItems)
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            var model = new EditTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                CurrentAttachmentPath = task.AttachmentPath,
                // Convert DB items to simple list for the view
                ChecklistItems = task.TodoItems.Select(i => i.Description).ToList(),
                // Get IDs of currently assigned users
                SelectedUserIds = task.Assignments.Select(a => a.ApplicationUserId).ToList(),
                // Load ALL users for the modal selection
                AllUsers = _userManager.Users.ToList()
            };

            return View(model);
        }

        // 4. EDIT TASK 
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var task = await _context.Tasks
                    .Include(t => t.TodoItems)
                    .Include(t => t.Assignments)
                    .FirstOrDefaultAsync(t => t.Id == model.Id);

                if (task == null) return NotFound();

                // 1. Update Basic Info
                task.Title = model.Title;
                task.Description = model.Description;
                task.DueDate = model.DueDate;
                task.Priority = model.Priority;

                // 2. Handle File Upload (Replace Old)
                if (model.Attachment != null)
                {
                    // Delete old file
                    if (!string.IsNullOrEmpty(task.AttachmentPath))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, "attachments", task.AttachmentPath);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Save new file
                    string folder = Path.Combine(_env.WebRootPath, "attachments");
                    string fileName = Guid.NewGuid().ToString() + "_" + model.Attachment.FileName;
                    using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }
                    task.AttachmentPath = fileName;
                }

                // 3. Update Checklist (Clear old, Add new)
                _context.TodoItems.RemoveRange(task.TodoItems); // Remove existing
                if (model.ChecklistItems != null)
                {
                    foreach (var item in model.ChecklistItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                            _context.TodoItems.Add(new TodoItem { Description = item, AppTaskId = task.Id });
                    }
                }

                // 4. Update Assignments (Clear old, Add new)
                _context.TaskAssignments.RemoveRange(task.Assignments); // Remove existing
                if (model.SelectedUserIds != null)
                {
                    foreach (var userId in model.SelectedUserIds)
                    {
                        _context.TaskAssignments.Add(new TaskAssignment { AppTaskId = task.Id, ApplicationUserId = userId });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // If fail, reload users
            model.AllUsers = _userManager.Users.ToList();
            return View(model);
        }

        // 5. MY TASKS 
        public async Task<IActionResult> MyTasks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var myTasks = await _context.Tasks
                .Include(t => t.Assignments)
                .Include(t => t.TodoItems)
                // FILTER: Only show tasks where the current user is in the assignment list
                .Where(t => t.Assignments.Any(a => a.ApplicationUserId == user.Id))
                .OrderByDescending(t => t.DueDate)
                .ToListAsync();

            return View(myTasks);
        }

        // 6. UPDATE STATUS
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, AppTaskStatus status)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                task.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MyTasks));
        }

        // 7. TOGGLE CHECKLIST ITEM
        [HttpPost]
        public async Task<IActionResult> ToggleTodo(int id)
        {
            var item = await _context.TodoItems
                .Include(t => t.AppTask)
                .ThenInclude(t => t.TodoItems)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (item == null) return NotFound();

            // 1. Toggle Status
            item.IsCompleted = !item.IsCompleted;

            // 2. Calculate Progress of Parent Task
            var task = item.AppTask;
            int total = task.TodoItems.Count;

            // Count items where IsCompleted is true
            int done = task.TodoItems.Count(t => t.IsCompleted);

            // 3. Update Task Status based on Progress
            if (done == 0)
                task.Status = AppTaskStatus.Pending;
            else if (done == total)
                task.Status = AppTaskStatus.Completed;
            else
                task.Status = AppTaskStatus.InProgress;

            await _context.SaveChangesAsync();

            // Return JSON
            return Json(new
            {
                success = true,
                taskId = task.Id,
                progress = total == 0 ? 0 : (int)((double)done / total * 100),
                status = task.Status.ToString(),
                isCompleted = item.IsCompleted // Send back the new state
            });
        }
    }
}