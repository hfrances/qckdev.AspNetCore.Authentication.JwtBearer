namespace JwtBearerExample.Identity.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "JwtBearerExample.Identity";

    public string Audience { get; set; } = "JwtBearerExample.Identity.Api";

    public string Key { get; set; } = string.Empty;

    public int AccessExpireMinutes { get; set; } = 60;
}
