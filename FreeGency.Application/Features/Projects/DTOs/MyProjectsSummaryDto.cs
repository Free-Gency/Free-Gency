namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class MyProjectsSummaryDto
    {
        public int Total { get; init; }
        public int Draft { get; init; }
        public int Open { get; init; }
        public int InProgress { get; init; }
        public int Completed { get; init; }
        public int Cancelled { get; init; }
        public IReadOnlyList<ProjectDto> UpcomingDeadlines { get; init; } = [];
    }
}
