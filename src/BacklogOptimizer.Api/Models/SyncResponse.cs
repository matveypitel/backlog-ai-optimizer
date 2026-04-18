namespace BacklogOptimizer.Api.Models;

public sealed record SyncResponse(int SyncedCount, IReadOnlyList<string> Errors);
