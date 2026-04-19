using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Embeddings;

public interface IEmbeddingSyncService
{
    Task<Result<EmbeddingSyncResult>> SyncJiraEmbeddingsAsync(CancellationToken cancellationToken = default);
    Task<Result<EmbeddingSyncResult>> SyncPageEmbeddingsAsync(CancellationToken cancellationToken = default);
}
