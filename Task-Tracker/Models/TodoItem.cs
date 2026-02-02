using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTracker.Models
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsCompleted { get; set; } = false;

        public int AppTaskId { get; set; }
        // We use 'virtual' for Lazy Loading if needed later
        [ForeignKey("AppTaskId")]
        public virtual AppTask AppTask { get; set; }
    }
}