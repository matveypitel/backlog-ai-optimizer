namespace BacklogOptimizer.Api.Models;

public sealed record SyncStatusResponse(
    int TotalIssues,
    DateTime? LastSyncedAt);
