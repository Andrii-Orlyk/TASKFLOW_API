namespace TaskFlow.Application.DTOs.Dashboard;

public record DashboardSummaryResponse(
    int TotalProjects,
    int TotalTasks,
    int TodoTasks,
    int InProgressTasks,
    int DoneTasks,
    int OverdueTasks);
