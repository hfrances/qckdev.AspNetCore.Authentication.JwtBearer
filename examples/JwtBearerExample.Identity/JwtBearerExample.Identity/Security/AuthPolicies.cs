namespace JwtBearerExample.Identity.Security;

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string SupportOnly = "SupportOnly";
    public const string AdminOrSupport = "AdminOrSupport";
}
