using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace TaskTracker.Models
{
    public class AppTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public AppTaskStatus Status { get; set; } = AppTaskStatus.Pending;
        public string? AttachmentPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<TaskAssignment> Assignments { get; set; }
        public virtual ICollection<TodoItem> TodoItems { get; set; }
    }
}