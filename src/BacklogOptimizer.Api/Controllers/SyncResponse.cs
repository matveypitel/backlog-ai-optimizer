namespace BacklogOptimizer.Api.Controllers;

public sealed record SyncResponse(int SyncedCount, IReadOnlyList<string> Errors);
