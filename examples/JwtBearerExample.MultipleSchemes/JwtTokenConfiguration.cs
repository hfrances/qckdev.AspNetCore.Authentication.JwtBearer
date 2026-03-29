namespace JwtBearerExample.MultipleSchemes
{
    public sealed class JwtTokenConfiguration
    {
        public string Key { get; set; } = string.Empty;

        public double? AccessExpireSeconds { get; set; }
    }
}
