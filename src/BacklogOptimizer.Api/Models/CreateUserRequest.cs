using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.Api.Models;

public record CreateUserRequest(string Email, string Password, Role Role);
