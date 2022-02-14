using System;

namespace qckdev.AspNetCore.Authentication.JwtBearer
{
    public sealed class JwtToken
    {
        public string AccessToken { get; set; }
        public DateTime Expired { get; set; }
        public string RefreshToken { get; set; }
    }
}