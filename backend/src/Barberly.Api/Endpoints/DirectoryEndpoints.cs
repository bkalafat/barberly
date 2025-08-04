using Barberly.Application.Directory.Commands;
using Barberly.Application.Directory.Queries;
using Barberly.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Barberly.Api.Endpoints;

public static class DirectoryEndpoints
{
    public static void MapDirectoryEndpoints(this WebApplication app)
    {
        var directory = app.MapGroup("/api/v1").WithTags("Directory");

        // BarberShop endpoints
        directory.MapPost("/shops", CreateBarberShop)
            .WithName("CreateBarberShop")
            .WithOpenApi()
            .RequireAuthorization();

        directory.MapGet("/shops", GetBarberShops)
            .WithName("GetBarberShops")
            .WithOpenApi()
            .AllowAnonymous();

        directory.MapGet("/shops/{id:guid}", GetBarberShopById)
            .WithName("GetBarberShopById")
            .WithOpenApi()
            .AllowAnonymous();

        directory.MapPut("/shops/{id:guid}", UpdateBarberShop)
            .WithName("UpdateBarberShop")
            .WithOpenApi()
            .RequireAuthorization();

        directory.MapDelete("/shops/{id:guid}", DeleteBarberShop)
            .WithName("DeleteBarberShop")
            .WithOpenApi()
            .RequireAuthorization();

        directory.MapGet("/shops/nearby", SearchNearbyBarberShops)
            .WithName("SearchNearbyBarberShops")
            .WithOpenApi()
            .AllowAnonymous();

        // Barber endpoints
        directory.MapPost("/barbers", CreateBarber)
            .WithName("CreateBarber")
            .WithOpenApi()
            .RequireAuthorization();

        directory.MapGet("/barbers", GetBarbers)
            .WithName("GetBarbers")
            .WithOpenApi()
            .AllowAnonymous();

        directory.MapGet("/barbers/{id:guid}", GetBarberById)
            .WithName("GetBarberById")
            .WithOpenApi()
            .AllowAnonymous();

        directory.MapPut("/barbers/{id:guid}", UpdateBarber)
            .WithName("UpdateBarber")
            .WithOpenApi()
            .RequireAuthorization();

        directory.MapDelete("/barbers/{id:guid}", DeleteBarber)
            .WithName("DeleteBarber")
            .WithOpenApi()
            .RequireAuthorization();

        // Service endpoints
        directory.MapPost("/services", CreateService)
            .WithName("CreateService")
            .WithOpenApi()
            .RequireAuthorization();

        directory.MapGet("/services", GetServices)
            .WithName("GetServices")
            .WithOpenApi()
            .AllowAnonymous();

        directory.MapGet("/services/{id:guid}", GetServiceById)
            .WithName("GetServiceById")
            .WithOpenApi()
            .AllowAnonymous();

        directory.MapPut("/services/{id:guid}", UpdateService)
            .WithName("UpdateService")
            .WithOpenApi()
            .RequireAuthorization();

        directory.MapDelete("/services/{id:guid}", DeleteService)
            .WithName("DeleteService")
            .WithOpenApi()
            .RequireAuthorization();
    }

    // BarberShop handlers
    private static async Task<IResult> CreateBarberShop(
        [FromBody] CreateBarberShopRequest request,
        ISender sender)
    {
        try
        {
            var command = new CreateBarberShopCommand(
                request.Name,
                request.Description,
                new CreateAddressCommand(
                    request.Address.Street,
                    request.Address.City,
                    request.Address.State,
                    request.Address.PostalCode,
                    request.Address.Country),
                request.Phone,
                request.Email,
                request.Website,
                request.OpenTime,
                request.CloseTime,
                request.WorkingDays,
                request.Latitude,
                request.Longitude);

            var result = await sender.Send(command);
            return Results.Created($"/api/v1/shops/{result}", new { id = result });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetBarberShops(
        ISender sender,
        [FromQuery] decimal? latitude,
        [FromQuery] decimal? longitude,
        [FromQuery] double? radiusKm,
        [FromQuery] string? serviceName,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new Barberly.Application.Directory.Queries.GetBarberShopsQuery(latitude, longitude, radiusKm, serviceName, minPrice, maxPrice, page, pageSize);
            var result = await sender.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            // Return error details for debugging
            return Results.Problem(detail: ex.ToString(), statusCode: 500);
        }
    }

    private static async Task<IResult> GetBarberShopById(Guid id, ISender sender)
    {
        try
        {
            var query = new GetBarberShopByIdQuery(id);
            var result = await sender.Send(query);
            return result != null ? Results.Ok(result) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.ToString(), statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateBarberShop(
        Guid id,
        [FromBody] UpdateBarberShopRequest request,
        ISender sender)
    {
        var command = new UpdateBarberShopCommand(id, request.Name, request.Description, request.Phone, request.Email, request.Website);
        var result = await sender.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> DeleteBarberShop(Guid id, ISender sender)
    {
        var command = new DeleteBarberShopCommand(id);
        var result = await sender.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> SearchNearbyBarberShops(
        ISender sender,
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] double radiusKm = 10.0,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new SearchNearbyBarberShopsQuery(latitude, longitude, radiusKm, page, pageSize);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    // Barber handlers
    private static async Task<IResult> CreateBarber(
        [FromBody] CreateBarberRequest request,
        ISender sender)
    {
        try
        {
            var command = new CreateBarberCommand(
                request.FullName,
                request.Email,
                request.Phone,
                request.BarberShopId,
                request.YearsOfExperience,
                request.Bio);

            var result = await sender.Send(command);
            return Results.Created($"/api/v1/barbers/{result}", new { id = result });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetBarbers(
        ISender sender,
        [FromQuery] Guid? barberShopId,
        [FromQuery] string? serviceName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new Application.Directory.Queries.GetBarbersQuery(barberShopId, serviceName, page, pageSize);
            var result = await sender.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.ToString(), statusCode: 500);
        }
    }

    private static async Task<IResult> GetBarberById(Guid id, ISender sender)
    {
        try
        {
            var query = new GetBarberByIdQuery(id);
            var result = await sender.Send(query);
            return result != null ? Results.Ok(result) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.ToString(), statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateBarber(
        Guid id,
        [FromBody] UpdateBarberRequest request,
        ISender sender)
    {
        var command = new UpdateBarberCommand(id, request.FullName, request.Email, request.Phone, request.Bio);
        var result = await sender.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> DeleteBarber(Guid id, ISender sender)
    {
        var command = new DeleteBarberCommand(id);
        var result = await sender.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    // Service handlers
    private static async Task<IResult> CreateService(
        [FromBody] CreateServiceRequest request,
        ISender sender)
    {
        try
        {
            var command = new CreateServiceCommand(
                request.Name,
                request.Description,
                request.Price,
                request.DurationInMinutes,
                request.BarberShopId);

            var result = await sender.Send(command);
            return Results.Created($"/api/v1/services/{result}", new { id = result });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetServices(
        ISender sender,
        [FromQuery] Guid? barberShopId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minDurationMinutes,
        [FromQuery] int? maxDurationMinutes,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new Application.Directory.Queries.GetServicesQuery(barberShopId, minPrice, maxPrice, minDurationMinutes, maxDurationMinutes, page, pageSize);
            var result = await sender.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.ToString(), statusCode: 500);
        }
    }

    private static async Task<IResult> GetServiceById(Guid id, ISender sender)
    {
        try
        {
            var query = new GetServiceByIdQuery(id);
            var result = await sender.Send(query);
            return result != null ? Results.Ok(result) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.ToString(), statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateService(
        Guid id,
        [FromBody] UpdateServiceRequest request,
        ISender sender)
    {
        var command = new UpdateServiceCommand(id, request.Name, request.Description, request.Price, request.DurationInMinutes);
        var result = await sender.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> DeleteService(Guid id, ISender sender)
    {
        var command = new DeleteServiceCommand(id);
        var result = await sender.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }
}
