using qckdev.Authentication.JwtBearer;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JwtBearerExample.SingleSchemeWithRoles
{
    public interface IJwtGeneratorService
    {
        Task<JwtToken> CreateTokenAsync(string scheme, string userName, IEnumerable<string> roles, IEnumerable<Claim> claims, CancellationToken cancellationToken);
    }
}
