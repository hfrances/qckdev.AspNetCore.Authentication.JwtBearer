using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using qckdev.AspNetCore.Authentication.JwtBearer;
using qckdev.Authentication.JwtBearer;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JwtBearerExample.SingleSchemeWithRoles
{
    public sealed class JwtGeneratorService : IJwtGeneratorService
    {
        private readonly IOptionsMonitor<JwtBearerOptions> _jwtOptionsMonitor;
        private readonly IOptionsMonitor<JwtBearerMoreOptions> _jwtMoreOptionsMonitor;

        public JwtGeneratorService(
            IOptionsMonitor<JwtBearerOptions> jwtOptionsMonitor,
            IOptionsMonitor<JwtBearerMoreOptions> jwtMoreOptionsMonitor)
        {
            _jwtOptionsMonitor = jwtOptionsMonitor;
            _jwtMoreOptionsMonitor = jwtMoreOptionsMonitor;
        }

        public Task<JwtToken> CreateTokenAsync(string scheme, string userName, IEnumerable<string> roles, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var jwtOptions = _jwtOptionsMonitor.Get(scheme);
            var jwtMoreOptions = _jwtMoreOptionsMonitor.Get(scheme);

            return Task.FromResult(
                JwtGenerator.CreateToken(
                    jwtOptions.TokenValidationParameters.IssuerSigningKey,
                    userName,
                    roles,
                    claims,
                    jwtMoreOptions.TokenLifeTimespan));
        }
    }
}
