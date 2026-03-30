using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace JwtBearerExample.MultipleSchemes.Common
{
    public static class DependencyInjection
    {
        const string BASE_PATH_KEY = "BasePath";

        public static IApplicationBuilder UseConfiguredBasePath(this IApplicationBuilder app, IConfiguration configuration)
        {
            var basePathRaw = configuration.GetSection(BASE_PATH_KEY)?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(basePathRaw))
            {
                return app;
            }

            var basePaths = basePathRaw
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));

            foreach (var basePath in basePaths)
            {
                app.UsePathBase(basePath);
            }

            return app;
        }
    }
}
