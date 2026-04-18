namespace BacklogOptimizer.Application.Jira;

public record JiraSyncResult(int SyncedCount, IReadOnlyList<string> Errors);
