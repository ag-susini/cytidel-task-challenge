namespace Tasker.Application.DTOs;

public record AuthResultDto(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    string? Error);