namespace BacklogOptimizer.Application.Jira;

public interface IJiraSyncService
{
    Task<JiraSyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
