using Xunit;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Server.IntegrationTests;

public sealed class EndpointResolutionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndpointResolutionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/modules/endpoints")]
    [InlineData("/api/modules/installed")]
    [InlineData("/api/system-settings")]
    public async Task ModuleEndpoint_ReturnsOk(string route)
    {
        var response = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/health/module-catalog")]
    [InlineData("/api/health/grocery-store")]
    [InlineData("/api/health/system-settings")]
    public async Task ModuleHealthEndpoint_ReturnsOk(string route)
    {
        var response = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}