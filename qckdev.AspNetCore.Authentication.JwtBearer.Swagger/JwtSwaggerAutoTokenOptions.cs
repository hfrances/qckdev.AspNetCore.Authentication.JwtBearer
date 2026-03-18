using System;
using System.Collections.Generic;

namespace qckdev.AspNetCore.Authentication.JwtBearer.Swagger
{
    /// <summary>
    /// Configures how Swagger UI captures JWT tokens from API responses and applies them to authorization schemes.
    /// </summary>
    public sealed class JwtSwaggerAutoTokenOptions
    {
        /// <summary>
        /// Gets the response capture rules.
        /// </summary>
        public IList<JwtSwaggerAutoTokenCaptureRule> Rules { get; } = new List<JwtSwaggerAutoTokenCaptureRule>();

        /// <summary>
        /// Adds or replaces a response capture rule.
        /// </summary>
        /// <param name="schemeName">OpenAPI security scheme name.</param>
        /// <param name="endpointPath">Endpoint path that returns the token.</param>
        /// <param name="configure">Optional rule configuration callback.</param>
        /// <returns>The same options instance.</returns>
        public JwtSwaggerAutoTokenOptions AddRule(
            string schemeName,
            string endpointPath,
            Action<JwtSwaggerAutoTokenCaptureRule>? configure = null)
        {
            if (string.IsNullOrWhiteSpace(schemeName))
            {
                throw new ArgumentNullException(nameof(schemeName));
            }

            if (string.IsNullOrWhiteSpace(endpointPath))
            {
                throw new ArgumentNullException(nameof(endpointPath));
            }

            var settings = new JwtSwaggerAutoTokenCaptureRule
            {
                SchemeName = schemeName,
                EndpointPath = endpointPath
            };

            configure?.Invoke(settings);

            for (var i = 0; i < Rules.Count; i++)
            {
                if (string.Equals(Rules[i].SchemeName, schemeName, StringComparison.Ordinal) &&
                    string.Equals(Rules[i].EndpointPath, endpointPath, StringComparison.Ordinal))
                {
                    Rules[i] = settings;
                    return this;
                }
            }

            Rules.Add(settings);
            return this;
        }
    }

    /// <summary>
    /// Defines how a token should be captured from a specific endpoint response.
    /// </summary>
    public sealed class JwtSwaggerAutoTokenCaptureRule
    {
        /// <summary>
        /// OpenAPI security scheme name to authorize when token is captured.
        /// </summary>
        public string SchemeName { get; set; } = "Bearer";

        /// <summary>
        /// Endpoint path to match (for example, /security/jwt/token).
        /// </summary>
        public string EndpointPath { get; set; } = string.Empty;

        /// <summary>
        /// HTTP method to match. Default is POST.
        /// </summary>
        public string HttpMethod { get; set; } = "POST";

        /// <summary>
        /// Dot notation path to the token value in the JSON response (for example, accessToken or data.token).
        /// </summary>
        public string TokenJsonPath { get; set; } = "accessToken";

        /// <summary>
        /// Removes 'Bearer ' prefix from captured token values when present.
        /// </summary>
        public bool StripBearerPrefix { get; set; } = true;

        /// <summary>
        /// Logs capture progress into the browser console when true.
        /// </summary>
        public bool Log { get; set; }
    }
}
