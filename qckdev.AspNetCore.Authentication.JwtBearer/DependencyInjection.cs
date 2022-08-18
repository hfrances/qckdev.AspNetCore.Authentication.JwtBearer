using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using qckdev.AspNetCore.Authentication.JwtBearer;
using System;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Extension methods to configure JWT bearer authentication.
    /// </summary>
    public static class QAspNetCoreAuthenticationJwtBearer
    {

        /// <summary>
        /// Enables JWT-bearer authentication using the specified scheme. 
        /// JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="configureOptions">A delegate that allows configuring <see cref="JwtBearerOptions"/>.</param>
        /// <param name="configureMoreOptions">A delegate that allows configuring <see cref="JwtBearerMoreOptions"/>.</param>
        /// <returns>A reference to builder after the operation has completed.</returns>
        public static AuthenticationBuilder AddJwtBearer(this AuthenticationBuilder builder, Action<JwtBearerOptions> configureOptions, Action<JwtBearerMoreOptions> configureMoreOptions)
        {
            return AddJwtBearer(builder, JwtBearerDefaults.AuthenticationScheme, configureOptions, configureMoreOptions);
        }

        /// <summary>
        /// Enables JWT-bearer authentication using the specified scheme. 
        /// JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header.
        /// </summary>
        /// <typeparam name="TJwtBearerMoreOptions">The type of the additional Jwt options.</typeparam>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="configureOptions">A delegate that allows configuring <see cref="JwtBearerOptions"/>.</param>
        /// <param name="configureMoreOptions">A delegate that allows configuring <typeparamref name="TJwtBearerMoreOptions"/>.</param>
        /// <returns>A reference to builder after the operation has completed.</returns>
        public static AuthenticationBuilder AddJwtBearer<TJwtBearerMoreOptions>(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtBearerOptions> configureOptions, Action<TJwtBearerMoreOptions> configureMoreOptions)
            where TJwtBearerMoreOptions : JwtBearerMoreOptions
        {
            return AddJwtBearer(builder, authenticationScheme, null, configureOptions, configureMoreOptions);
        }

        /// <summary>
        /// Enables JWT-bearer authentication using the specified scheme. 
        /// JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="configureOptions">A delegate that allows configuring <see cref="JwtBearerOptions"/>.</param>
        /// <param name="configureMoreOptions">A delegate that allows configuring <see cref="JwtBearerMoreOptions"/>.</param>
        /// <returns>A reference to builder after the operation has completed.</returns>
        public static AuthenticationBuilder AddJwtBearer(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtBearerOptions> configureOptions, Action<JwtBearerMoreOptions> configureMoreOptions)
        {
            return AddJwtBearer(builder, authenticationScheme, null, configureOptions, configureMoreOptions);
        }

        /// <summary>
        /// Enables JWT-bearer authentication using the specified scheme. 
        /// JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header.
        /// </summary>
        /// <typeparam name="TJwtBearerMoreOptions">The type of the additional Jwt options.</typeparam>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="configureOptions">A delegate that allows configuring <see cref="JwtBearerOptions"/>.</param>
        /// <param name="configureMoreOptions">A delegate that allows configuring <typeparamref name="TJwtBearerMoreOptions"/>.</param>
        /// <returns>A reference to builder after the operation has completed.</returns>
        public static AuthenticationBuilder AddJwtBearer<TJwtBearerMoreOptions>(this AuthenticationBuilder builder, Action<JwtBearerOptions> configureOptions, Action<TJwtBearerMoreOptions> configureMoreOptions)
            where TJwtBearerMoreOptions : JwtBearerMoreOptions
        {
            return AddJwtBearer(builder, JwtBearerDefaults.AuthenticationScheme, configureOptions, configureMoreOptions);
        }

        /// <summary>
        /// Enables JWT-bearer authentication using the specified scheme. 
        /// JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="displayName">The display name for the authentication handler.</param>
        /// <param name="configureOptions">A delegate that allows configuring <see cref="JwtBearerOptions"/>.</param>
        /// <param name="configureMoreOptions">A delegate that allows configuring <see cref="JwtBearerMoreOptions"/>.</param>
        /// <returns>A reference to builder after the operation has completed.</returns>
        public static AuthenticationBuilder AddJwtBearer(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<JwtBearerOptions> configureOptions, Action<JwtBearerMoreOptions> configureMoreOptions)
        {
            builder.Services.Configure(authenticationScheme, configureMoreOptions);
            return builder.AddJwtBearer(authenticationScheme, displayName, configureOptions);
        }

        /// <summary>
        /// Enables JWT-bearer authentication using the specified scheme. 
        /// JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header.
        /// </summary>
        /// <typeparam name="TJwtBearerMoreOptions">The type of the additional Jwt options.</typeparam>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="displayName">The display name for the authentication handler.</param>
        /// <param name="configureOptions">A delegate that allows configuring <see cref="JwtBearerOptions"/>.</param>
        /// <param name="configureMoreOptions">A delegate that allows configuring <typeparamref name="TJwtBearerMoreOptions"/>.</param>
        /// <returns>A reference to builder after the operation has completed.</returns>
        public static AuthenticationBuilder AddJwtBearer<TJwtBearerMoreOptions>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<JwtBearerOptions> configureOptions, Action<TJwtBearerMoreOptions> configureMoreOptions)
            where TJwtBearerMoreOptions : JwtBearerMoreOptions
        {
            builder.Services.Configure(authenticationScheme, configureMoreOptions);
            return builder.AddJwtBearer(authenticationScheme, displayName, configureOptions);
        }

    }
}