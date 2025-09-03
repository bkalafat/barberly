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
        // Use a known barber ID from the seeded data
        var barberId = new Guid("9e862653-65c0-41b0-82e9-754f638baa49"); // Ahmet Yılmaz
        var url = $"/api/v1/barbers/{barberId}/availability?date={DateTime.UtcNow:yyyy-MM-dd}";
        Console.WriteLine($"Testing URL: {url}");
        var res = await client.GetAsync(url);
        Console.WriteLine($"Response status: {(int)res.StatusCode} - {res.ReasonPhrase}");
        var body = await res.Content.ReadAsStringAsync();
        Console.WriteLine($"Response body: {body}");
        Assert.True(res.IsSuccessStatusCode, $"Expected successful status code from availability endpoint. Got {(int)res.StatusCode} - {res.ReasonPhrase}. Body: {body}");
    }

    [Fact]
    public async Task PostAppointment_WithIdempotencyKey_Returns_201_or_200()
    {
        var client = _factory.CreateClient();

        // First register and login to get a token
        var token = await GetAuthTokenForRole(client, "customer");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Use known IDs from seeded data
        var barberId = new Guid("9e862653-65c0-41b0-82e9-754f638baa49"); // Ahmet Yılmaz
        var serviceId = new Guid("1a2b3c4d-5e6f-7890-1234-567890abcdef"); // Use a valid GUID format for service
        var req = new
        {
            UserId = Guid.NewGuid(),
            BarberId = barberId,
            ServiceId = serviceId,
            Start = DateTimeOffset.UtcNow.AddHours(24),
            End = DateTimeOffset.UtcNow.AddHours(24).AddMinutes(30)
        };
        var content = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/appointments");
        message.Content = content;
        message.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var res = await client.SendAsync(message);
        if (res.StatusCode != System.Net.HttpStatusCode.Created && res.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var body = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"POST /appointments failed: {(int)res.StatusCode} - {res.ReasonPhrase}\n{body}");
        }
        Assert.True(res.StatusCode == System.Net.HttpStatusCode.Created || res.StatusCode == System.Net.HttpStatusCode.OK, "Expected 201 Created or 200 OK from create appointment");
    }

    private async Task<string> GetAuthTokenForRole(HttpClient client, string role)
    {
        var email = $"role-test-{role}-{Guid.NewGuid()}@example.com";
        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FullName = $"Test {role}",
            Role = role
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        await client.PostAsync("/auth/register", registerContent);

        var loginRequest = new
        {
            Email = email,
            Password = registerRequest.Password
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync("/auth/login", loginContent);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);

        return loginData.GetProperty("token").GetString()!;
    }
}
