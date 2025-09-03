using Barberly.Application.Directory.Commands;
using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using Barberly.Domain.ValueObjects;
using MediatR;

namespace Barberly.Application.Directory.Handlers;

public class CreateBarberShopCommandHandler : IRequestHandler<CreateBarberShopCommand, Guid>
{
    private readonly IBarberShopRepository _barberShopRepository;

    public CreateBarberShopCommandHandler(IBarberShopRepository barberShopRepository)
    {
        _barberShopRepository = barberShopRepository;
    }

    public async Task<Guid> Handle(CreateBarberShopCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(
            request.Address.Street,
            request.Address.City,
            request.Address.State,
            request.Address.PostalCode,
            request.Address.Country);

        var barberShop = BarberShop.Create(
            request.Name,
            request.Description,
            address,
            request.Phone,
            request.Email,
            request.OpenTime,
            request.CloseTime,
            request.WorkingDays,
            request.Latitude,
            request.Longitude,
            request.Website);

        await _barberShopRepository.AddAsync(barberShop, cancellationToken);

        return barberShop.Id;
    }
}

public class CreateBarberCommandHandler : IRequestHandler<CreateBarberCommand, Guid>
{
    private readonly IBarberRepository _barberRepository;

    public CreateBarberCommandHandler(IBarberRepository barberRepository)
    {
        _barberRepository = barberRepository;
    }

    public async Task<Guid> Handle(CreateBarberCommand request, CancellationToken cancellationToken)
    {
        var barber = Barber.Create(
            request.FullName,
            request.Email,
            request.Phone,
            request.BarberShopId,
            request.YearsOfExperience,
            request.Bio);

        await _barberRepository.AddAsync(barber, cancellationToken);

        return barber.Id;
    }
}

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
{
    private readonly IServiceRepository _serviceRepository;

    public CreateServiceCommandHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = Service.Create(
            request.Name,
            request.Description,
            request.Price,
            request.DurationInMinutes,
            request.BarberShopId);

        await _serviceRepository.AddAsync(service, cancellationToken);

        return service.Id;
    }
}

public class UpdateBarberShopCommandHandler : IRequestHandler<UpdateBarberShopCommand, bool>
{
    private readonly IBarberShopRepository _barberShopRepository;

    public UpdateBarberShopCommandHandler(IBarberShopRepository barberShopRepository)
    {
        _barberShopRepository = barberShopRepository;
    }

    public async Task<bool> Handle(UpdateBarberShopCommand request, CancellationToken cancellationToken)
    {
        var barberShop = await _barberShopRepository.GetByIdAsync(request.Id, cancellationToken);
        if (barberShop == null) return false;

        barberShop.UpdateInfo(request.Name, request.Description, request.Phone, request.Email, request.Website);
        await _barberShopRepository.UpdateAsync(barberShop, cancellationToken);

        return true;
    }
}

public class UpdateBarberCommandHandler : IRequestHandler<UpdateBarberCommand, bool>
{
    private readonly IBarberRepository _barberRepository;

    public UpdateBarberCommandHandler(IBarberRepository barberRepository)
    {
        _barberRepository = barberRepository;
    }

    public async Task<bool> Handle(UpdateBarberCommand request, CancellationToken cancellationToken)
    {
        var barber = await _barberRepository.GetByIdAsync(request.Id, cancellationToken);
        if (barber == null) return false;

        barber.UpdateInfo(request.FullName, request.Email, request.Phone, request.Bio);
        await _barberRepository.UpdateAsync(barber, cancellationToken);

        return true;
    }
}

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, bool>
{
    private readonly IServiceRepository _serviceRepository;

    public UpdateServiceCommandHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<bool> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (service == null) return false;

        service.UpdateInfo(request.Name, request.Description, request.Price, request.DurationInMinutes);
        await _serviceRepository.UpdateAsync(service, cancellationToken);

        return true;
    }
}

public class DeleteBarberShopCommandHandler : IRequestHandler<DeleteBarberShopCommand, bool>
{
    private readonly IBarberShopRepository _barberShopRepository;

    public DeleteBarberShopCommandHandler(IBarberShopRepository barberShopRepository)
    {
        _barberShopRepository = barberShopRepository;
    }

    public async Task<bool> Handle(DeleteBarberShopCommand request, CancellationToken cancellationToken)
    {
        var barberShop = await _barberShopRepository.GetByIdAsync(request.Id, cancellationToken);
        if (barberShop == null) return false;

        barberShop.Deactivate();
        await _barberShopRepository.UpdateAsync(barberShop, cancellationToken);

        return true;
    }
}

public class DeleteBarberCommandHandler : IRequestHandler<DeleteBarberCommand, bool>
{
    private readonly IBarberRepository _barberRepository;

    public DeleteBarberCommandHandler(IBarberRepository barberRepository)
    {
        _barberRepository = barberRepository;
    }

    public async Task<bool> Handle(DeleteBarberCommand request, CancellationToken cancellationToken)
    {
        var barber = await _barberRepository.GetByIdAsync(request.Id, cancellationToken);
        if (barber == null) return false;

        barber.Deactivate();
        await _barberRepository.UpdateAsync(barber, cancellationToken);

        return true;
    }
}

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
{
    private readonly IServiceRepository _serviceRepository;

    public DeleteServiceCommandHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (service == null) return false;

        service.Deactivate();
        await _serviceRepository.UpdateAsync(service, cancellationToken);

        return true;
    }
}
