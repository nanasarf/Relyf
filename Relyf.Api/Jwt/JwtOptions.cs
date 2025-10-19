namespace Relyf.Api.Jwt;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string Key { get; init; } = "";           // Base64 in user-secrets
    public int AccessTokenMinutes { get; init; } = 60;
}
