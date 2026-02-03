using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Models; 

namespace TaskTracker.Data
{
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