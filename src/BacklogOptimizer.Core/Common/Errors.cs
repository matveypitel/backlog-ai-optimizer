namespace BacklogOptimizer.Core.Common;

public static class Errors
{
    public static class Validation
    {
        public static Error UrlRequired => new(
            "validation.url_required",
            ErrorType.Validation,
            "URL must be a non-empty value.");

        public static Error UrlMalformed(string url) => new(
            "validation.url_malformed",
            ErrorType.Validation,
            $"URL '{url}' is not a well-formed absolute URI.");
    }

    public static class NotFound
    {
        public static Error JiraIssue(string jiraKey) => new(
            "not_found.jira_issue",
            ErrorType.NotFound,
            $"Jira issue '{jiraKey}' was not found.");

        public static Error JiraIssueEmbedding(string jiraKey) => new(
            "not_found.jira_issue_embedding",
            ErrorType.NotFound,
            $"Jira issue '{jiraKey}' has no embedding. Run sync first.");
    }

    public static class Jira
    {
        public static Error ApiFailure(string message) => new(
            "jira.api_failure",
            ErrorType.Validation,
            $"Jira API call failed: {message}");
    }

    public static class Llm
    {
        public static Error CallFailed(string message) => new(
            "llm.call_failed",
            ErrorType.Validation,
            $"LLM call failed: {message}");

        public static Error ResponseInvalid(string message) => new(
            "llm.response_invalid",
            ErrorType.Validation,
            $"Failed to parse LLM response: {message}");
    }
}
