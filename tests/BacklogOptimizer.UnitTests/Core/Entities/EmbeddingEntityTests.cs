using BacklogOptimizer.Core.Entities;

using Pgvector;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class EmbeddingEntityTests
{
    private static Vector MakeVector() => new Vector(new float[] { 0.1f, 0.2f, 0.3f });

    [Fact]
    public void JiraIssueEmbedding_Constructor_SetsAllProperties()
    {
        var issueId = Guid.NewGuid();
        var vector = MakeVector();
        var before = DateTime.UtcNow;

        var embedding = new JiraIssueEmbedding(issueId, "text-embedding-3-small", vector);

        Assert.Equal(issueId, embedding.JiraIssueId);
        Assert.Equal("text-embedding-3-small", embedding.ModelName);
        Assert.Equal(vector, embedding.Vector);
        Assert.True(embedding.EmbeddedAt >= before);
    }

    [Fact]
    public void JiraIssueEmbedding_Update_ChangesModelNameAndVector()
    {
        var embedding = new JiraIssueEmbedding(Guid.NewGuid(), "old-model", MakeVector());
        var newVector = new Vector(new float[] { 0.9f, 0.8f });
        var before = DateTime.UtcNow;

        embedding.Update("new-model", newVector);

        Assert.Equal("new-model", embedding.ModelName);
        Assert.Equal(newVector, embedding.Vector);
        Assert.True(embedding.EmbeddedAt >= before);
    }

    [Fact]
    public void CompetitorFeatureEmbedding_Constructor_SetsAllProperties()
    {
        var featureId = Guid.NewGuid();
        var vector = MakeVector();
        var before = DateTime.UtcNow;

        var embedding = new CompetitorFeatureEmbedding(featureId, "text-embedding-3-small", vector);

        Assert.Equal(featureId, embedding.CompetitorFeatureId);
        Assert.Equal("text-embedding-3-small", embedding.ModelName);
        Assert.Equal(vector, embedding.Vector);
        Assert.True(embedding.EmbeddedAt >= before);
    }

    [Fact]
    public void CompetitorFeatureEmbedding_Update_ChangesModelNameAndVector()
    {
        var embedding = new CompetitorFeatureEmbedding(Guid.NewGuid(), "old-model", MakeVector());
        var newVector = new Vector(new float[] { 0.5f });
        var before = DateTime.UtcNow;

        embedding.Update("new-model", newVector);

        Assert.Equal("new-model", embedding.ModelName);
        Assert.Equal(newVector, embedding.Vector);
        Assert.True(embedding.EmbeddedAt >= before);
    }
}
