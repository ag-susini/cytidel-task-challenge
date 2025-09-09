namespace Tasker.Application.Services;

public class JwtSettings
{
    public string Issuer { get; set; } = "Tasker";
    public string Audience { get; set; } = "TaskerClient";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}