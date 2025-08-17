using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Barberly.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Barberly.IntegrationTests.Scheduling;

public class SchedulingEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SchedulingEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAvailability_Returns_OK()
    {
        var client = _factory.CreateClient();
        var barberId = Guid.NewGuid();
        var res = await client.GetAsync($"/api/v1/barbers/{barberId}/availability?date={DateTime.UtcNow:yyyy-MM-dd}");
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"GET /availability failed: {(int)res.StatusCode} - {res.ReasonPhrase}\n{body}");
        }
        Assert.True(res.IsSuccessStatusCode, "Expected successful status code from availability endpoint");
    }

    [Fact]
    public async Task PostAppointment_WithIdempotencyKey_Returns_201_or_200()
    {
        var client = _factory.CreateClient();
        // get a test token
        var tokenRes = await client.PostAsync("/auth/test-token?email=test@local&role=Customer", null);
        string token = string.Empty;
        if (tokenRes.IsSuccessStatusCode)
        {
            var tokenJson = await tokenRes.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(tokenJson);
            if (doc.RootElement.TryGetProperty("token", out var t)) token = t.GetString() ?? string.Empty;
        }
        var req = new
        {
            UserId = Guid.NewGuid(),
            BarberId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            Start = DateTimeOffset.UtcNow.AddHours(24),
            End = DateTimeOffset.UtcNow.AddHours(24).AddMinutes(30)
        };
        var content = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/appointments");
        message.Content = content;
        message.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        if (!string.IsNullOrEmpty(token))
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var res = await client.SendAsync(message);
        if (res.StatusCode != System.Net.HttpStatusCode.Created && res.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var body = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"POST /appointments failed: {(int)res.StatusCode} - {res.ReasonPhrase}\n{body}");
        }
        Assert.True(res.StatusCode == System.Net.HttpStatusCode.Created || res.StatusCode == System.Net.HttpStatusCode.OK, "Expected 201 Created or 200 OK from create appointment");
    }
}
