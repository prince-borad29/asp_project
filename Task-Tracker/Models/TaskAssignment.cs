using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTracker.Models
{
    public class TaskAssignment
    {
        [Key]
        public int Id { get; set; }

        public int AppTaskId { get; set; }
        [ForeignKey("AppTaskId")]
        public virtual AppTask AppTask { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}