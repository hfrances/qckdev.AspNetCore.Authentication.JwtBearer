namespace JwtBearerExample.SingleScheme
{
    public sealed class JwtTokenConfiguration
    {
        public string Key { get; set; } = string.Empty;

        public double? AccessExpireSeconds { get; set; }
    }
}
