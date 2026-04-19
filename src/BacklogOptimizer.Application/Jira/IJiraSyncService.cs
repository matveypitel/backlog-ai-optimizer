using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Jira;

public interface IJiraSyncService
{
    Task<Result<JiraSyncResult>> SyncAsync(CancellationToken cancellationToken = default);
}
