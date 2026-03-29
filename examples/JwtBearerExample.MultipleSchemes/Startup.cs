using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JwtBearerExample.MultipleSchemes
{
    public class Startup
    {
        public const string AUTHENTICATIONSCHEME_CODE = "Code";
        public const string AUTHENTICATIONSCHEME_TOKEN = "Bearer";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var jwtCodeConfiguration = Configuration.GetSection("OAuth2:Code").Get<JwtTokenConfiguration>() ?? new JwtTokenConfiguration();
            var jwtTokenConfiguration = Configuration.GetSection("OAuth2:Token").Get<JwtTokenConfiguration>() ?? new JwtTokenConfiguration();

            services.AddScoped<IJwtGeneratorService, JwtGeneratorService>();

            services
                .AddAuthentication(AUTHENTICATIONSCHEME_TOKEN)
                .AddJwtBearer(
                    AUTHENTICATIONSCHEME_CODE,
                    options => ConfigureJwtBearerOptions(options, jwtCodeConfiguration),
                    moreOptions => ConfigureMoreOptions(moreOptions, jwtCodeConfiguration))
                .AddJwtBearer(
                    AUTHENTICATIONSCHEME_TOKEN,
                    options => ConfigureJwtBearerOptions(options, jwtTokenConfiguration),
                    moreOptions => ConfigureMoreOptions(moreOptions, jwtTokenConfiguration));

            services.AddAuthorization();
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtBearer Multiple Schemes API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JwtBearer Multiple Schemes API v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void ConfigureJwtBearerOptions(JwtBearerOptions options, JwtTokenConfiguration configuration)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.Key));
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        private static void ConfigureMoreOptions(qckdev.AspNetCore.Authentication.JwtBearer.JwtBearerMoreOptions options, JwtTokenConfiguration configuration)
        {
            options.TokenLifeTimespan = configuration.AccessExpireSeconds.HasValue
                ? TimeSpan.FromSeconds(configuration.AccessExpireSeconds.Value)
                : (TimeSpan?)null;
        }
    }
}
