namespace TaskTracker.Models.ViewModels
{
    public class EditTaskViewModel : CreateTaskViewModel
    {
        public int Id { get; set; }
        public string? CurrentAttachmentPath { get; set; }
    }
}