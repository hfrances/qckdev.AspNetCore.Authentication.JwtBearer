using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.AspNetCore.Authentication.JwtBearer;
using System;

namespace qckdev.AspNetCore.Authentication.JwtBearer.Test
{
    [TestClass]
    public class DependencyInjectionTest
    {
        sealed class CustomMoreOptions : JwtBearerMoreOptions
        {
            public string CustomValue { get; set; }
        }

        [TestMethod]
        public void AddJwtBearer_WithDefaultScheme_ConfiguresJwtAndMoreOptions()
        {
            var services = new ServiceCollection();

            services
                .AddAuthentication()
                .AddJwtBearer(
                    options => options.RequireHttpsMetadata = false,
                    moreOptions => moreOptions.TokenLifeTimespan = TimeSpan.FromMinutes(30)
                );

            var provider = services.BuildServiceProvider();
            var jwtOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
            var moreOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerMoreOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

            Assert.IsFalse(jwtOptions.RequireHttpsMetadata);
            Assert.AreEqual(TimeSpan.FromMinutes(30), moreOptions.TokenLifeTimespan);
        }

        [TestMethod]
        public void AddJwtBearer_WithNamedScheme_ConfiguresNamedOptions()
        {
            const string scheme = "Code";
            var services = new ServiceCollection();

            services
                .AddAuthentication()
                .AddJwtBearer(
                    scheme,
                    options => options.SaveToken = true,
                    moreOptions => moreOptions.TokenLifeTimespan = TimeSpan.FromSeconds(45)
                );

            var provider = services.BuildServiceProvider();
            var jwtOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(scheme);
            var moreOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerMoreOptions>>().Get(scheme);

            Assert.IsTrue(jwtOptions.SaveToken);
            Assert.AreEqual(TimeSpan.FromSeconds(45), moreOptions.TokenLifeTimespan);
        }

        [TestMethod]
        public void AddJwtBearer_GenericMoreOptions_RegistersDerivedOptionsType()
        {
            const string scheme = "Custom";
            var services = new ServiceCollection();

            services
                .AddAuthentication()
                .AddJwtBearer<CustomMoreOptions>(
                    scheme,
                    options => options.RequireHttpsMetadata = false,
                    moreOptions =>
                    {
                        moreOptions.TokenLifeTimespan = TimeSpan.FromMinutes(10);
                        moreOptions.CustomValue = "abc";
                    }
                );

            var provider = services.BuildServiceProvider();
            var moreOptions = provider.GetRequiredService<IOptionsMonitor<CustomMoreOptions>>().Get(scheme);

            Assert.AreEqual(TimeSpan.FromMinutes(10), moreOptions.TokenLifeTimespan);
            Assert.AreEqual("abc", moreOptions.CustomValue);
        }

        [TestMethod]
        public void AddJwtBearer_WithDisplayName_ConfiguresBothOptionSets()
        {
            const string scheme = "Bearer2";
            var services = new ServiceCollection();

            services
                .AddAuthentication()
                .AddJwtBearer(
                    scheme,
                    "Bearer 2",
                    options => options.RequireHttpsMetadata = false,
                    moreOptions => moreOptions.TokenLifeTimespan = TimeSpan.FromDays(1)
                );

            var provider = services.BuildServiceProvider();
            var jwtOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(scheme);
            var moreOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerMoreOptions>>().Get(scheme);

            Assert.IsFalse(jwtOptions.RequireHttpsMetadata);
            Assert.AreEqual(TimeSpan.FromDays(1), moreOptions.TokenLifeTimespan);
        }
    }
}
