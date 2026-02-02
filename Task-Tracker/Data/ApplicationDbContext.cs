using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Models; // Ensure this using statement is here

namespace TaskTracker.Data
{
    // Notice we pass <ApplicationUser> here 👇
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppTask> Tasks { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
    }
}