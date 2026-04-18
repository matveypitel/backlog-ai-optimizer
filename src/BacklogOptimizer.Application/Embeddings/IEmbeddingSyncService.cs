namespace BacklogOptimizer.Application.Embeddings;

public interface IEmbeddingSyncService
{
    Task<EmbeddingSyncResult> SyncJiraEmbeddingsAsync(CancellationToken cancellationToken = default);
    Task<EmbeddingSyncResult> SyncPageEmbeddingsAsync(CancellationToken cancellationToken = default);
}
