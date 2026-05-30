using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.UnitTests.Core.Common;

public class ErrorsTests
{
    [Fact]
    public void Validation_UrlRequired_HasCorrectFields()
    {
        var error = Errors.Validation.UrlRequired;

        Assert.Equal("validation.url_required", error.Id);
        Assert.Equal(ErrorType.Validation, error.Type);
    }

    [Fact]
    public void Validation_UrlMalformed_ContainsUrl()
    {
        var error = Errors.Validation.UrlMalformed("bad-url");

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Contains("bad-url", error.Description);
    }

    [Fact]
    public void NotFound_JiraIssue_ContainsKey()
    {
        var error = Errors.NotFound.JiraIssue("PROJ-1");

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Contains("PROJ-1", error.Description);
    }

    [Fact]
    public void NotFound_FeatureSuggestion_ContainsId()
    {
        var id = Guid.NewGuid();
        var error = Errors.NotFound.FeatureSuggestion(id);

        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void Auth_InvalidCredentials_IsUnauthorized()
    {
        var error = Errors.Auth.InvalidCredentials;

        Assert.Equal(ErrorType.Unauthorized, error.Type);
        Assert.Equal("auth.invalid_credentials", error.Id);
    }

    [Fact]
    public void Auth_EmailAlreadyExists_ContainsEmail()
    {
        var error = Errors.Auth.EmailAlreadyExists("test@example.com");

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Contains("test@example.com", error.Description);
    }

    [Fact]
    public void Auth_UserNotFound_ContainsId()
    {
        var id = Guid.NewGuid();
        var error = Errors.Auth.UserNotFound(id);

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void ScrapingSource_UrlAlreadyExists_IsConflict()
    {
        var error = Errors.ScrapingSource.UrlAlreadyExists("https://example.com");

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains("https://example.com", error.Description);
    }

    [Fact]
    public void ScrapingSource_NotFound_ContainsId()
    {
        var id = Guid.NewGuid();
        var error = Errors.ScrapingSource.NotFound(id);

        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void Jira_ApiFailure_ContainsMessage()
    {
        var error = Errors.Jira.ApiFailure("connection refused");

        Assert.Contains("connection refused", error.Description);
    }

    [Fact]
    public void Llm_CallFailed_ContainsMessage()
    {
        var error = Errors.Llm.CallFailed("timeout");

        Assert.Contains("timeout", error.Description);
    }

    [Fact]
    public void Llm_ResponseInvalid_ContainsMessage()
    {
        var error = Errors.Llm.ResponseInvalid("parse error");

        Assert.Contains("parse error", error.Description);
    }
}
