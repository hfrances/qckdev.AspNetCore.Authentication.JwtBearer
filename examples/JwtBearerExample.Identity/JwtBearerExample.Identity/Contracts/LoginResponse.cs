namespace JwtBearerExample.Identity.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserName,
    IReadOnlyCollection<string> Roles);
