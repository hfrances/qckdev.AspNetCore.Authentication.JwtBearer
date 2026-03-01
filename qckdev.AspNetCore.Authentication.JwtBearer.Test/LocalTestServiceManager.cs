using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using qckdev.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace qckdev.AspNetCore.Authentication.JwtBearer.Test
{
    internal static class LocalTestServiceManager
    {
        private static readonly object SyncLock = new object();
        private static IHost? Host;
        private static bool Initialized;
        private static readonly Uri BaseUri = new Uri($"http://localhost:{GetPortForCurrentProcess()}/");
        private static readonly string TestSecretKey = "test-secret-key-that-is-very-long-enough-for-hs256";
        private static readonly byte[] TestKeyBytes = Encoding.UTF8.GetBytes(TestSecretKey);

        public static Uri ServiceUri => BaseUri;

        public static string GenerateValidToken(
            string? subject = "testuser",
            TimeSpan? lifetime = null,
            bool expired = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(TestKeyBytes);

            DateTime issuedAt;
            DateTime expirationTime;

            if (expired)
            {
                // Create a token that was issued 2 hours ago and expired 1 hour ago
                issuedAt = DateTime.UtcNow.AddHours(-2);
                expirationTime = DateTime.UtcNow.AddHours(-1);
            }
            else
            {
                issuedAt = DateTime.UtcNow;
                expirationTime = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, subject ?? "testuser"),
                new Claim(ClaimTypes.Name, subject ?? "testuser")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims, "JWT"),
                IssuedAt = issuedAt,
                NotBefore = issuedAt,
                Expires = expirationTime,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static void StartIfNeeded()
        {
            lock (SyncLock)
            {
                if (Initialized)
                {
                    return;
                }

                var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls(BaseUri.ToString());
                        webBuilder.ConfigureServices(services =>
                        {
                            services.AddControllers().AddApplicationPart(typeof(JwtBearerAuthenticationTestController).Assembly);

                            var key = new SymmetricSecurityKey(TestKeyBytes);

                            services
                                .AddAuthentication()
                                .AddJwtBearer(opts =>
                                {
                                    opts.RequireHttpsMetadata = false;
                                    opts.TokenValidationParameters = new TokenValidationParameters
                                    {
                                        ValidateIssuerSigningKey = true,
                                        IssuerSigningKey = key,
                                        ValidateIssuer = false,
                                        ValidateAudience = false,
                                        ValidateLifetime = true,
                                        ClockSkew = TimeSpan.Zero
                                    };
                                }, moreOpts =>
                                {
                                    moreOpts.TokenLifeTimespan = TimeSpan.FromHours(1);
                                });

                            services.AddAuthorization();
                        });

                        webBuilder.Configure(app =>
                        {
                            app.UseRouting();
                            app.UseAuthentication();
                            app.UseAuthorization();
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllers();
                            });
                        });
                    });

                Host = builder.Build();
                Host.Start();

                if (!WaitForService(10000))
                {
                    Stop();
                    throw new InvalidOperationException($"Test service did not become ready at {BaseUri}");
                }

                Initialized = true;
            }
        }

        public static void Stop()
        {
            lock (SyncLock)
            {
                try
                {
                    Host?.StopAsync().GetAwaiter().GetResult();
                    Host?.Dispose();
                }
                finally
                {
                    Host = null;
                    Initialized = false;
                }
            }
        }

        private static bool WaitForService(int timeoutMs)
        {
            using var client = new HttpClient { BaseAddress = BaseUri };
            var started = DateTime.UtcNow;

            while ((DateTime.UtcNow - started).TotalMilliseconds < timeoutMs)
            {
                try
                {
                    var response = client.GetAsync("jwt/public").GetAwaiter().GetResult();
                    var statusCode = (int)response.StatusCode;
                    if (statusCode >= 200 && statusCode < 500)
                    {
                        return true;
                    }
                }
                catch
                {
                }

                Thread.Sleep(150);
            }

            return false;
        }

        private static int GetPortForCurrentProcess()
        {
            var pid = Process.GetCurrentProcess().Id;
            return 27000 + (pid % 10000);
        }
    }
}
