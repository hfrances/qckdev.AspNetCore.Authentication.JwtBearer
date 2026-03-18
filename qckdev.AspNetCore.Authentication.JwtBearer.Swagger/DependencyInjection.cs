using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace qckdev.AspNetCore.Authentication.JwtBearer.Swagger
{
    /// <summary>
    /// Extension methods to enrich Swagger UI with automatic JWT token capture.
    /// </summary>
    public static class QJwtBearerSwaggerDependencyInjection
    {
        const string DefaultAssetsRequestPath = "/swagger-ui/jwt-bearer";

        /// <summary>
        /// Injects JS that captures token responses and applies Swagger authorization automatically.
        /// </summary>
        /// <param name="options">Swagger UI options.</param>
        /// <param name="assetsRequestPath">Request path used to serve embedded assets.</param>
        /// <returns>The same options instance.</returns>
        public static SwaggerUIOptions UseJwtBearerSwaggerUi(
            this SwaggerUIOptions options,
            string assetsRequestPath = DefaultAssetsRequestPath)
            => UseJwtBearerSwaggerUi(options, configure: null, assetsRequestPath);

        /// <summary>
        /// Injects JS that captures token responses and applies Swagger authorization automatically.
        /// </summary>
        /// <param name="options">Swagger UI options.</param>
        /// <param name="configure">Configures token acquisition behavior by scheme.</param>
        /// <param name="assetsRequestPath">Request path used to serve embedded assets.</param>
        /// <returns>The same options instance.</returns>
        public static SwaggerUIOptions UseJwtBearerSwaggerUi(
            this SwaggerUIOptions options,
            Action<JwtSwaggerAutoTokenOptions>? configure,
            string assetsRequestPath = DefaultAssetsRequestPath)
        {
            var path = NormalizeRequestPath(assetsRequestPath);
            var clientConfig = BuildClientConfig(configure);
            var cfg = Uri.EscapeDataString(JsonSerializer.Serialize(clientConfig));
            options.InjectJavascript($"{path}/jwt-bearer-auth.js?cfg={cfg}");
            return options;
        }

        /// <summary>
        /// Serves embedded Swagger UI assets required by <see cref="UseJwtBearerSwaggerUi(SwaggerUIOptions, string)"/>.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="assetsRequestPath">Request path used to serve embedded assets.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseJwtBearerSwaggerUiStaticFiles(
            this IApplicationBuilder app,
            string assetsRequestPath = DefaultAssetsRequestPath)
        {
            var path = NormalizeRequestPath(assetsRequestPath);
            var assembly = Assembly.GetExecutingAssembly();
            var jsResourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(".wwwroot.jwt-bearer-auth.js", StringComparison.OrdinalIgnoreCase));
            var jsPath = path + "/jwt-bearer-auth.js";

            app.Use(async (context, next) =>
            {
                var requestPath = context.Request.Path.Value ?? string.Empty;
                if (requestPath.Equals(jsPath, StringComparison.OrdinalIgnoreCase))
                {
                    await WriteEmbeddedResourceAsync(context, assembly, jsResourceName, "application/javascript").ConfigureAwait(false);
                    return;
                }

                await next().ConfigureAwait(false);
            });

            return app;
        }

        static async Task WriteEmbeddedResourceAsync(
            Microsoft.AspNetCore.Http.HttpContext context,
            Assembly assembly,
            string? resourceName,
            string contentType)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                context.Response.StatusCode = 404;
                return;
            }

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            context.Response.ContentType = contentType;
            context.Response.StatusCode = 200;

            using (stream)
            {
                await stream.CopyToAsync(context.Response.Body).ConfigureAwait(false);
            }
        }

        static string NormalizeRequestPath(string path)
        {
            var value = string.IsNullOrWhiteSpace(path)
                ? DefaultAssetsRequestPath
                : path.Trim();

            if (!value.StartsWith("/", StringComparison.Ordinal))
            {
                value = "/" + value;
            }

            return value.TrimEnd('/');
        }

        static JwtSwaggerAutoTokenClientConfig BuildClientConfig(Action<JwtSwaggerAutoTokenOptions>? configure)
        {
            var options = new JwtSwaggerAutoTokenOptions();
            configure?.Invoke(options);

            return new JwtSwaggerAutoTokenClientConfig
            {
                Rules = options.Rules
                    .Where(x => !string.IsNullOrWhiteSpace(x.SchemeName) && !string.IsNullOrWhiteSpace(x.EndpointPath))
                    .Select(x => new JwtSwaggerAutoTokenClientRuleConfig
                    {
                        SchemeName = x.SchemeName,
                        EndpointPath = x.EndpointPath,
                        HttpMethod = string.IsNullOrWhiteSpace(x.HttpMethod) ? "POST" : x.HttpMethod,
                        TokenJsonPath = string.IsNullOrWhiteSpace(x.TokenJsonPath) ? "accessToken" : x.TokenJsonPath,
                        StripBearerPrefix = x.StripBearerPrefix,
                        Log = x.Log
                    })
                    .ToList()
            };
        }

        sealed class JwtSwaggerAutoTokenClientConfig
        {
            public List<JwtSwaggerAutoTokenClientRuleConfig> Rules { get; set; } = new List<JwtSwaggerAutoTokenClientRuleConfig>();
        }

        sealed class JwtSwaggerAutoTokenClientRuleConfig
        {
            public string SchemeName { get; set; } = string.Empty;
            public string EndpointPath { get; set; } = string.Empty;
            public string HttpMethod { get; set; } = "POST";
            public string TokenJsonPath { get; set; } = "accessToken";
            public bool StripBearerPrefix { get; set; } = true;
            public bool Log { get; set; }
        }
    }
}
