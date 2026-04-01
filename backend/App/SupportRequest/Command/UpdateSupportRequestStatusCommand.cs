using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;

namespace StockApp.App.SupportRequest.Command;

public record UpdateSupportRequestStatusCommand(int Id, SupportRequestStatus Status) : IRequest<bool>;

internal class UpdateSupportRequestStatusCommandHandler : IRequestHandler<UpdateSupportRequestStatusCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public UpdateSupportRequestStatusCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateSupportRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SupportRequests.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return false;

        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
