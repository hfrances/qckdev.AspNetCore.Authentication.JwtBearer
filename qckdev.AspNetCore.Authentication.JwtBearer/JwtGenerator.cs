using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace qckdev.AspNetCore.Authentication.JwtBearer
{
    public static class JwtGenerator
    {

        public static JwtToken CreateToken(SecurityKey key, string userName, IEnumerable<string> roles = null, IEnumerable<Claim> claims = null, TimeSpan? lifespan = null)
        {
            var tokenClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, userName)
            };
            roles?.ToList().ForEach(rol => tokenClaims.Add(new Claim(ClaimTypes.Role, rol)));
            claims?.ToList().ForEach(val => tokenClaims.Add(val));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(tokenClaims),
                Expires = DateTime.UtcNow.Add(lifespan ?? TimeSpan.FromDays(1)),
                SigningCredentials = credentials,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescription);
            return new JwtToken
            {
                AccessToken = tokenHandler.WriteToken(token),
                Expired = token.ValidTo,
                RefreshToken = $"1/{CreateGenericToken()}"
            };
        }

        public static JwtSecurityToken ValidateToken(TokenValidationParameters validationParameters, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return (JwtSecurityToken)validatedToken;
        }

        public static string CreateGenericToken()
        {
            var randomNumber = GetRandomSerie();

            return Convert.ToBase64String(randomNumber)
                //Avoid trouble with routes
                .Replace("/", "!");
        }

        private static byte[] GetRandomSerie()
        {
            var randomNumber = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return randomNumber;
        }

    }
}
