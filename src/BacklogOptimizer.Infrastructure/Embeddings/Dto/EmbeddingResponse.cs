namespace BacklogOptimizer.Infrastructure.Embeddings.Dto;

internal sealed record EmbeddingResponse(EmbeddingDataItem[] Data);

internal sealed record EmbeddingDataItem(float[] Embedding, int Index);
