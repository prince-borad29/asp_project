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

        // 📁 File Upload
        public IFormFile? Attachment { get; set; }

        // 👥 Multi-Select Users (Holds the IDs of selected people)
        public List<string> SelectedUserIds { get; set; } = new List<string>();

        // This list will populate the Modal
        public List<ApplicationUser> AllUsers { get; set; } = new List<ApplicationUser>();

        // ✅ Dynamic Checklist (We will receive a list of strings from the View)
        public List<string> ChecklistItems { get; set; } = new List<string>();
    }
}