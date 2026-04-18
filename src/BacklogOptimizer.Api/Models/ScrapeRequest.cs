using System.ComponentModel.DataAnnotations;

namespace BacklogOptimizer.Api.Models;

public sealed record ScrapeRequest([Required][Url] string Url);
