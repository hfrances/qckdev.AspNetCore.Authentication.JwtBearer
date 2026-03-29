using System.Collections.Generic;

namespace JwtBearerExample.SingleScheme
{
    public sealed class TokenRequest
    {
        public string UserName { get; set; } = "demo.user";

        public List<string> Roles { get; set; } = new List<string>();
    }
}
