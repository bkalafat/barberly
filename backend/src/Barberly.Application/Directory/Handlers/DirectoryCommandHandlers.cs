using Barberly.Application.Directory.Commands;
using Barberly.Domain.Entities;
using Barberly.Domain.ValueObjects;
using MediatR;

namespace Barberly.Application.Directory.Handlers;

public class CreateBarberShopCommandHandler : IRequestHandler<CreateBarberShopCommand, Guid>
{
    // Note: Repository interfaces will be added when Infrastructure layer is created
    // For now, this is the structure following Clean Architecture

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

        // TODO: Add to repository
        // await _barberShopRepository.AddAsync(barberShop, cancellationToken);
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        return barberShop.Id;
    }
}

public class CreateBarberCommandHandler : IRequestHandler<CreateBarberCommand, Guid>
{
    public async Task<Guid> Handle(CreateBarberCommand request, CancellationToken cancellationToken)
    {
        var barber = Barber.Create(
            request.FullName,
            request.Email,
            request.Phone,
            request.BarberShopId,
            request.YearsOfExperience,
            request.Bio);

        // TODO: Add to repository
        // await _barberRepository.AddAsync(barber, cancellationToken);
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        return barber.Id;
    }
}

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = Service.Create(
            request.Name,
            request.Description,
            request.Price,
            request.DurationInMinutes,
            request.BarberShopId);

        // TODO: Add to repository
        // await _serviceRepository.AddAsync(service, cancellationToken);
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        return service.Id;
    }
}

public class UpdateBarberShopCommandHandler : IRequestHandler<UpdateBarberShopCommand, bool>
{
    public async Task<bool> Handle(UpdateBarberShopCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get from repository, update, and save
        // var barberShop = await _barberShopRepository.GetByIdAsync(request.Id, cancellationToken);
        // if (barberShop == null) return false;
        
        // barberShop.UpdateInfo(request.Name, request.Description, request.Phone, request.Email, request.Website);
        
        // await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

public class UpdateBarberCommandHandler : IRequestHandler<UpdateBarberCommand, bool>
{
    public async Task<bool> Handle(UpdateBarberCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get from repository, update, and save
        return true;
    }
}

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, bool>
{
    public async Task<bool> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get from repository, update, and save
        return true;
    }
}

public class DeleteBarberShopCommandHandler : IRequestHandler<DeleteBarberShopCommand, bool>
{
    public async Task<bool> Handle(DeleteBarberShopCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get from repository, deactivate (soft delete), and save
        return true;
    }
}

public class DeleteBarberCommandHandler : IRequestHandler<DeleteBarberCommand, bool>
{
    public async Task<bool> Handle(DeleteBarberCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get from repository, deactivate, and save
        return true;
    }
}

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
{
    public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get from repository, deactivate, and save
        return true;
    }
}
