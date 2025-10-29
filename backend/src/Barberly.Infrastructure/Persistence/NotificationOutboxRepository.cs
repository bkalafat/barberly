using Barberly.Application.Interfaces;
using Barberly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barberly.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for NotificationOutbox entity.
/// </summary>
public sealed class NotificationOutboxRepository : INotificationOutboxRepository
{
    private readonly BarberlyDbContext _context;

    public NotificationOutboxRepository(BarberlyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationOutbox notification, CancellationToken cancellationToken = default)
    {
        await _context.NotificationOutbox.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationOutbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationOutbox
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<List<NotificationOutbox>> GetPendingAsync(
        int batchSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await _context.NotificationOutbox
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(NotificationOutbox notification, CancellationToken cancellationToken = default)
    {
        _context.NotificationOutbox.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.NotificationOutbox
            .CountAsync(n => n.Status == NotificationStatus.Pending, cancellationToken);
    }

    public async Task<List<NotificationOutbox>> GetFailedAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.NotificationOutbox
            .Where(n => n.Status == NotificationStatus.Failed)
            .OrderByDescending(n => n.UpdatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
