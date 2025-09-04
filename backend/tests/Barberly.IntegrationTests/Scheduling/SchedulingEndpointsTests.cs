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

        // Get actual barber ID from seeded data by querying the API
        var barbersResponse = await client.GetAsync("/api/v1/barbers");
        barbersResponse.EnsureSuccessStatusCode();
        var barbersJson = await barbersResponse.Content.ReadAsStringAsync();
        var barbers = JsonSerializer.Deserialize<JsonElement>(barbersJson);

        // Find first barber from the seeded data
        var firstBarber = barbers.EnumerateArray().FirstOrDefault();
        Assert.False(firstBarber.ValueKind == JsonValueKind.Undefined, "No barbers found in seeded data");
        var barberId = Guid.Parse(firstBarber.GetProperty("id").GetString()!);

        var url = $"/api/v1/barbers/{barberId}/availability?date={DateTime.UtcNow:yyyy-MM-dd}";
        Console.WriteLine($"Testing URL: {url} with barber: {firstBarber.GetProperty("fullName").GetString()}");
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

        // Get actual IDs from seeded data by querying the API
        var barbersResponse = await client.GetAsync("/api/v1/barbers");
        barbersResponse.EnsureSuccessStatusCode();
        var barbersJson = await barbersResponse.Content.ReadAsStringAsync();
        var barbers = JsonSerializer.Deserialize<JsonElement>(barbersJson);

        // Find Kadir Alkan from the seeded barbers (real Trabzon data)
        var kadirBarber = barbers.EnumerateArray()
            .FirstOrDefault(b => b.GetProperty("fullName").GetString() == "Kadir Alkan");
        Assert.False(kadirBarber.ValueKind == JsonValueKind.Undefined, "Could not find Kadir Alkan barber in seeded data");
        var barberId = Guid.Parse(kadirBarber.GetProperty("id").GetString()!);

        // Get services for this barber's shop
        var servicesResponse = await client.GetAsync($"/api/v1/services?barberShopId={kadirBarber.GetProperty("barberShopId").GetString()}");
        servicesResponse.EnsureSuccessStatusCode();
        var servicesJson = await servicesResponse.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<JsonElement>(servicesJson);

        // Use the first available service (Classic Haircut or Beard Trim)
        var firstService = services.EnumerateArray().First();
        var serviceId = Guid.Parse(firstService.GetProperty("id").GetString()!);
        var serviceDuration = firstService.GetProperty("durationInMinutes").GetInt32();

        // Use a future time slot to avoid conflicts (must be UTC for PostgreSQL compatibility)
        // Add random hours to prevent conflicts with previous test runs
        var randomHour = new Random().Next(8, 18); // Random hour between 8 AM and 6 PM
        var randomMinute = new Random().Next(0, 60); // Random minute
        var futureDate = DateTimeOffset.UtcNow.AddDays(new Random().Next(1, 7)); // Random day within next week
        var startTime = new DateTimeOffset(futureDate.Date.AddHours(randomHour).AddMinutes(randomMinute), TimeSpan.Zero);
        var endTime = startTime.AddMinutes(serviceDuration);

        var req = new
        {
            UserId = Guid.NewGuid(),
            BarberId = barberId,
            ServiceId = serviceId,
            Start = startTime,
            End = endTime
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
