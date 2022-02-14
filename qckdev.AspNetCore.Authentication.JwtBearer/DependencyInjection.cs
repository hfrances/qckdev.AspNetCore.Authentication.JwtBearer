using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using qckdev.AspNetCore.Authentication.JwtBearer;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class QAspNetCoreAuthenticationJwtBearer
    {

        public static AuthenticationBuilder AddJwtBearer(this AuthenticationBuilder builder, Action<JwtBearerOptions> configureOptions, Action<JwtBearerMoreOptions> configureMoreOptions)
        {
            return AddJwtBearer(builder, JwtBearerDefaults.AuthenticationScheme, configureOptions, configureMoreOptions);
        }

        public static AuthenticationBuilder AddJwtBearer<TJwtBearerMoreOptions>(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtBearerOptions> configureOptions, Action<TJwtBearerMoreOptions> configureMoreOptions)
            where TJwtBearerMoreOptions : JwtBearerMoreOptions
        {
            return AddJwtBearer(builder, authenticationScheme, null, configureOptions, configureMoreOptions);
        }

        public static AuthenticationBuilder AddJwtBearer(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtBearerOptions> configureOptions, Action<JwtBearerMoreOptions> configureMoreOptions)
        {
            return AddJwtBearer(builder, authenticationScheme, null, configureOptions, configureMoreOptions);
        }

        public static AuthenticationBuilder AddJwtBearer<TJwtBearerMoreOptions>(this AuthenticationBuilder builder, Action<JwtBearerOptions> configureOptions, Action<TJwtBearerMoreOptions> configureMoreOptions)
            where TJwtBearerMoreOptions : JwtBearerMoreOptions
        {
            return AddJwtBearer(builder, JwtBearerDefaults.AuthenticationScheme, configureOptions, configureMoreOptions);
        }

        public static AuthenticationBuilder AddJwtBearer(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<JwtBearerOptions> configureOptions, Action<JwtBearerMoreOptions> configureMoreOptions)
        {
            builder.Services.Configure(authenticationScheme, configureMoreOptions);
            return builder.AddJwtBearer(authenticationScheme, displayName, configureOptions);
        }

        public static AuthenticationBuilder AddJwtBearer<TJwtBearerMoreOptions>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<JwtBearerOptions> configureOptions, Action<TJwtBearerMoreOptions> configureMoreOptions)
            where TJwtBearerMoreOptions : JwtBearerMoreOptions
        {
            builder.Services.Configure(authenticationScheme, configureMoreOptions);
            return builder.AddJwtBearer(authenticationScheme, displayName, configureOptions);
        }

    }
}