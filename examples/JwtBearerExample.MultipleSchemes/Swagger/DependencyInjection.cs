using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using qckdev.AspNetCore.Authentication.JwtBearer.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace JwtBearerExample.MultipleSchemes.Swagger
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtBearer Multiple Schemes API", Version = "v1" });
                c.AddXmlCommentsFromCurrentAssembly();
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityDefinition("Code", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Code scheme."
                });

            });

            return services;
        }

        public static SwaggerGenOptions AddXmlCommentsFromCurrentAssembly(this SwaggerGenOptions options)
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            return options;
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
        {
            SwaggerBuilderExtensions.UseSwagger(app);
            app.UseJwtBearerSwaggerUiStaticFiles();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JwtBearer Multiple Schemes API v1");
                c.UseJwtBearerSwaggerUi(cfg =>
                {
                    cfg.AddRule(Startup.AUTHENTICATIONSCHEME_CODE, "/jwt/token/code", rule =>
                    {
                        rule.TokenJsonPath = "accessToken";
                        rule.Log = false;
                    });

                    cfg.AddRule(Startup.AUTHENTICATIONSCHEME_TOKEN, "/jwt/token/bearer", rule =>
                    {
                        rule.TokenJsonPath = "accessToken";
                        rule.Log = true;
                    });
                });
            });

            return app;
        }
    }
}
