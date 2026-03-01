using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;

namespace qckdev.AspNetCore.Authentication.JwtBearer.Test
{
    [TestClass]
    public class JwtBearerAuthenticationIntegrationTests
    {
        [TestMethod]
        public void PublicEndpoint_WithoutToken_ReturnsOk()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };

            var response = client.GetAsync("jwt/public").GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Assert.AreEqual("public-ok", content);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };

            var response = client.GetAsync("jwt/protected").GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithValidToken_ReturnsOk()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };
            using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/protected");

            var token = LocalTestServiceManager.GenerateValidToken();
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Assert.AreEqual("protected-ok-testuser", content);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithValidTokenAndCustomSubject_ReturnsOk()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };
            using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/protected");

            var token = LocalTestServiceManager.GenerateValidToken("customuser");
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Assert.AreEqual("protected-ok-customuser", content);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithExpiredToken_ReturnsUnauthorized()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };
            using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/protected");

            var expiredToken = LocalTestServiceManager.GenerateValidToken(expired: true);
            request.Headers.Add("Authorization", $"Bearer {expiredToken}");

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithMalformedToken_ReturnsUnauthorized()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };
            using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/protected");

            request.Headers.Add("Authorization", "Bearer invalid.token.here");

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithMissingBearerScheme_ReturnsUnauthorized()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };
            using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/protected");

            var token = LocalTestServiceManager.GenerateValidToken();
            request.Headers.Add("Authorization", token); // Missing "Bearer " prefix

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithWrongScheme_ReturnsUnauthorized()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };
            using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/protected");

            var token = LocalTestServiceManager.GenerateValidToken();
            request.Headers.Add("Authorization", $"Basic {token}"); // Wrong scheme

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void ProtectedEndpoint_WithValidTokenButCustomLifetime_ReturnsOk()
        {
            using var client = new HttpClient { BaseAddress = LocalTestServiceManager.ServiceUri };
            using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/protected");

            var token = LocalTestServiceManager.GenerateValidToken(lifetime: TimeSpan.FromSeconds(30));
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
