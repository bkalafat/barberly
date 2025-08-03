namespace Barberly.IntegrationTests.Auth;

/// <summary>
/// Authentication integration tests following Clean Architecture principles
/// Tests JWT-based auth as per copilot-instructions.md security requirements
/// </summary>
public class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task HealthCheck_Live_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_Ready_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new
        {
            Email = $"integration-test-{Guid.NewGuid()}@example.com",
            Password = "SecurePassword123!",
            FullName = "Integration Test User",
            Role = "customer"
        };

        var content = CreateJsonContent(request);

        // Act
        var response = await _client.PostAsync("/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("User registered successfully");
    }

    [Theory]
    [InlineData("invalid-email", "password123", "Test User", "customer")]
    [InlineData("test@example.com", "weak", "Test User", "customer")]
    [InlineData("test@example.com", "password123", "", "customer")]
    [InlineData("test@example.com", "password123", "Test User", "invalid-role")]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest(
        string email, string password, string fullName, string role)
    {
        // Arrange
        var request = new
        {
            Email = email,
            Password = password,
            FullName = fullName,
            Role = role
        };

        var content = CreateJsonContent(request);

        // Act
        var response = await _client.PostAsync("/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange - First register a user
        var email = $"login-test-{Guid.NewGuid()}@example.com";
        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FullName = "Login Test User",
            Role = "customer"
        };

        await _client.PostAsync("/auth/register", CreateJsonContent(registerRequest));

        var loginRequest = new
        {
            Email = email,
            Password = registerRequest.Password
        };

        // Act
        var response = await _client.PostAsync("/auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("token");
        responseContent.Should().Contain("Login successful");

        var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        loginResponse.TryGetProperty("token", out var tokenProperty).Should().BeTrue();
        tokenProperty.GetString().Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("nonexistent@example.com", "password123")]
    [InlineData("", "")]
    [InlineData("valid@example.com", "wrongpassword")]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest(string email, string password)
    {
        // Arrange
        var request = new
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsync("/auth/login", CreateJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("/me")]
    [InlineData("/customer-only")]
    [InlineData("/barber-only")]
    public async Task ProtectedEndpoints_WithoutAuth_ShouldReturnUnauthorized(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ShouldReturnOk()
    {
        // Arrange - Register and login to get token
        var token = await GetAuthTokenForRole("customer");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RoleBasedAccess_Customer_ShouldAccessCustomerEndpoint()
    {
        // Arrange - Register customer and get token
        var token = await GetAuthTokenForRole("customer");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/customer-only");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RoleBasedAccess_Customer_ShouldNotAccessBarberEndpoint()
    {
        // Arrange - Register customer and get token
        var token = await GetAuthTokenForRole("customer");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/barber-only");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private StringContent CreateJsonContent(object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<string> GetAuthTokenForRole(string role)
    {
        var email = $"role-test-{role}-{Guid.NewGuid()}@example.com";
        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FullName = $"Test {role}",
            Role = role
        };

        await _client.PostAsync("/auth/register", CreateJsonContent(registerRequest));

        var loginRequest = new
        {
            Email = email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsync("/auth/login", CreateJsonContent(loginRequest));
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginContent);

        return loginData.GetProperty("token").GetString()!;
    }
}
