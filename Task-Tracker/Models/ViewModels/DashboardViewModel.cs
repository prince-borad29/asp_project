using TaskTracker.Models;

namespace TaskTracker.Models.ViewModels
{
    public class DashboardViewModel
    {
        // 1. Counters
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }

        // 2. Chart Data (Arrays for Chart.js)
        public int[] StatusCounts { get; set; } // [Pending, InProgress, Completed]
        public int[] PriorityCounts { get; set; } // [Low, Medium, High]

        // 3. Recent Tasks List
        public List<AppTask> RecentTasks { get; set; }
    }
}