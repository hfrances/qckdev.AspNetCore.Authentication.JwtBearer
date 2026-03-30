using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JwtBearerExample.MultipleSchemes.Swagger
{
    [Obsolete("Temporary example filter. Replace it with qckdev.AspNetCore.Swagger.Filters.SecurityRequirementsOperationFilter when this branch is merged to master.")]
    sealed class SecurityRequirementsOperationFilter : IOperationFilter
    {
        readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        readonly IAuthorizationPolicyProvider _authorizationPolicyProvider;

        public SecurityRequirementsOperationFilter(
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IAuthorizationPolicyProvider authorizationPolicyProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _authorizationPolicyProvider = authorizationPolicyProvider;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || context == null)
            {
                return;
            }

            var authorizationData = GetAuthorizationData(context);
            if (authorizationData.AllowAnonymous)
            {
                operation.Security = new List<OpenApiSecurityRequirement>();
                return;
            }

            if (authorizationData.AuthorizeData.Count == 0)
            {
                return;
            }

            var availableSchemes = _authenticationSchemeProvider
                .GetAllSchemesAsync()
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var defaultSchemes = GetDefaultSchemes(availableSchemes);
            var schemeGroups = GetAuthorizeSchemeGroups(authorizationData.AuthorizeData, availableSchemes, defaultSchemes);
            if (schemeGroups.Count == 0)
            {
                var fallback = defaultSchemes.Count > 0
                    ? defaultSchemes
                    : availableSchemes.ToList();

                if (fallback.Count == 0)
                {
                    return;
                }

                schemeGroups.Add(fallback);
            }

            var combinations = ExpandCombinations(schemeGroups);
            if (combinations.Count == 0)
            {
                return;
            }

            operation.Security = new List<OpenApiSecurityRequirement>();
            foreach (var combination in combinations)
            {
                var requirement = new OpenApiSecurityRequirement();
                foreach (var scheme in combination.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    requirement[new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = scheme
                        }
                    }] = Array.Empty<string>();
                }

                if (requirement.Count > 0)
                {
                    operation.Security.Add(requirement);
                }
            }

            DeduplicateRequirements(operation.Security);
        }

        (bool AllowAnonymous, List<IAuthorizeData> AuthorizeData) GetAuthorizationData(OperationFilterContext context)
        {
            var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata ?? new List<object>();
            var hasAllowAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();
            var authorizeData = endpointMetadata.OfType<IAuthorizeData>().ToList();

            if (!hasAllowAnonymous && authorizeData.Count == 0)
            {
                hasAllowAnonymous = HasAllowAnonymous(context.MethodInfo);
                authorizeData = GetAuthorizeAttributes(context.MethodInfo).ToList();
            }

            return (hasAllowAnonymous, authorizeData);
        }

        List<List<string>> GetAuthorizeSchemeGroups(
            IReadOnlyCollection<IAuthorizeData> authorizeData,
            HashSet<string> availableSchemes,
            List<string> defaultSchemes)
        {
            var result = new List<List<string>>();
            foreach (var attribute in authorizeData)
            {
                var group = ParseSchemes(attribute.AuthenticationSchemes, availableSchemes);
                if (group.Count == 0)
                {
                    group = GetPolicySchemes(attribute.Policy, availableSchemes);
                }

                if (group.Count == 0)
                {
                    group = defaultSchemes;
                }

                if (group.Count > 0)
                {
                    result.Add(group);
                }
            }

            return result;
        }

        static IEnumerable<IAuthorizeData> GetAuthorizeAttributes(MethodInfo methodInfo)
        {
            var typeAttributes = methodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<IAuthorizeData>()
                ?? Enumerable.Empty<IAuthorizeData>();

            var methodAttributes = methodInfo
                .GetCustomAttributes(true)
                .OfType<IAuthorizeData>();

            return typeAttributes.Concat(methodAttributes);
        }

        static bool HasAllowAnonymous(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any()
                || methodInfo.DeclaringType?.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any() == true;
        }

        List<string> GetPolicySchemes(string? policyName, HashSet<string> availableSchemes)
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                return new List<string>();
            }

            var policy = _authorizationPolicyProvider.GetPolicyAsync(policyName)
                .GetAwaiter()
                .GetResult();

            if (policy == null)
            {
                return new List<string>();
            }

            return policy.AuthenticationSchemes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(availableSchemes.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        List<string> GetDefaultSchemes(HashSet<string> availableSchemes)
        {
            var candidates = new[]
            {
                _authenticationSchemeProvider.GetDefaultAuthenticateSchemeAsync().GetAwaiter().GetResult()?.Name,
                _authenticationSchemeProvider.GetDefaultChallengeSchemeAsync().GetAwaiter().GetResult()?.Name,
                _authenticationSchemeProvider.GetDefaultForbidSchemeAsync().GetAwaiter().GetResult()?.Name,
            };

            return candidates
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Where(availableSchemes.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        static List<string> ParseSchemes(string? authenticationSchemes, HashSet<string> availableSchemes)
        {
            return (authenticationSchemes ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Where(x => availableSchemes.Contains(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        static List<List<string>> ExpandCombinations(IReadOnlyCollection<List<string>> groups)
        {
            var result = new List<List<string>> { new List<string>() };

            foreach (var group in groups)
            {
                if (group.Count == 0)
                {
                    continue;
                }

                var current = new List<List<string>>();
                foreach (var prefix in result)
                {
                    foreach (var candidate in group)
                    {
                        var combination = new List<string>(prefix) { candidate };
                        current.Add(combination);
                    }
                }

                result = current;
            }

            return result;
        }

        static void DeduplicateRequirements(IList<OpenApiSecurityRequirement> requirements)
        {
            if (requirements.Count <= 1)
            {
                return;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = requirements.Count - 1; i >= 0; i--)
            {
                var key = string.Join("|", requirements[i]
                    .Select(x => x.Key.Reference?.Id ?? x.Key.Name ?? string.Empty)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

                if (!seen.Add(key))
                {
                    requirements.RemoveAt(i);
                }
            }
        }
    }
}
