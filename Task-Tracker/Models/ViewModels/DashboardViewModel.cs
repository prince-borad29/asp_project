using TaskTracker.Models;

namespace TaskTracker.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }

        public int[] StatusCounts { get; set; } 
        public int[] PriorityCounts { get; set; } 

        public List<AppTask> RecentTasks { get; set; }
    }
}