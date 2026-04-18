using System.ComponentModel.DataAnnotations;

namespace BacklogOptimizer.Api.Controllers;

public sealed record ScrapeRequest([Required] string Url);
