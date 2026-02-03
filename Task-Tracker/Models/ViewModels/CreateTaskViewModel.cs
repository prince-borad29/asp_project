using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models.ViewModels
{
    public class CreateTaskViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(1);

        public TaskPriority Priority { get; set; }

        public IFormFile? Attachment { get; set; }

        public List<string> SelectedUserIds { get; set; } = new List<string>();

        public List<ApplicationUser> AllUsers { get; set; } = new List<ApplicationUser>();

        public List<string> ChecklistItems { get; set; } = new List<string>();
    }
}