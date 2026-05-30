using System.Diagnostics.CodeAnalysis;

using Pgvector;

namespace BacklogOptimizer.Core.Entities;

public class JiraIssueEmbedding
{
    public Guid JiraIssueId { get; private set; }
    public string ModelName { get; private set; } = string.Empty;
    public Vector Vector { get; private set; } = null!;
    public DateTime EmbeddedAt { get; private set; }

    public JiraIssue JiraIssue { get; private set; } = null!;

    [ExcludeFromCodeCoverage]
    private JiraIssueEmbedding() { }

    public JiraIssueEmbedding(Guid jiraIssueId, string modelName, Vector vector)
    {
        JiraIssueId = jiraIssueId;
        ModelName = modelName;
        Vector = vector;
        EmbeddedAt = DateTime.UtcNow;
    }

    public void Update(string modelName, Vector vector)
    {
        ModelName = modelName;
        Vector = vector;
        EmbeddedAt = DateTime.UtcNow;
    }
}
