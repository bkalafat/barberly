using Barberly.Application.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Barberly.IntegrationTests.Directory;

public class DirectoryEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DirectoryEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetBarberShops_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/shops");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBarberShopById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/shops/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBarberShop_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateBarberShopRequest(
            "Test Barber Shop",
            "A great barber shop",
            new CreateAddressRequest("123 Main St", "Test City", "Test State", "12345", "Test Country"),
            "555-0123",
            "test@barbershop.com",
            "https://barbershop.com",
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
            40.7128m,
            -74.0060m);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/shops", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBarbers_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/barbers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBarberById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/barbers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBarber_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateBarberRequest(
            "John Doe",
            "john@barbershop.com",
            "555-0123",
            Guid.NewGuid(),
            5,
            "Experienced barber");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/barbers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetServices_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/services");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServiceById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/services/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateService_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateServiceRequest(
            "Haircut",
            "Professional haircut service",
            25.00m,
            30,
            Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/services", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchNearbyBarberShops_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/shops/nearby?latitude=40.7128&longitude=-74.0060&radiusKm=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBarberShops_WithQueryParameters_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/shops?page=1&pageSize=10&minPrice=20&maxPrice=100");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServices_WithQueryParameters_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/services?page=1&pageSize=10&minPrice=20&maxPrice=50&minDurationMinutes=15&maxDurationMinutes=60");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
