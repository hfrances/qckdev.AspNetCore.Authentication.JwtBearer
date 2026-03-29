using System.Collections.Generic;

namespace JwtBearerExample.MultipleSchemes
{
    public sealed class TokenRequest
    {
        public string UserName { get; set; } = "demo.user";

        public List<string> Roles { get; set; } = new List<string>();
    }
}
