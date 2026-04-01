using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;

namespace StockApp.App.SupportRequest.Query;

public record GetSupportRequestsQuery(SupportRequestStatus? Status = null, int Page = 1, int PageSize = 20)
    : IRequest<GetSupportRequestsResult>;

public record SupportRequestDto(
    int Id,
    string Email,
    string Subject,
    string? DetectedIntent,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetSupportRequestsResult(IReadOnlyList<SupportRequestDto> Items, int TotalCount);

internal class GetSupportRequestsQueryHandler : IRequestHandler<GetSupportRequestsQuery, GetSupportRequestsResult>
{
    private readonly ApplicationDbContext _context;

    public GetSupportRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetSupportRequestsResult> Handle(GetSupportRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SupportRequests.AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new SupportRequestDto(
                x.Id,
                x.Email,
                x.Subject,
                x.DetectedIntent,
                x.Status.ToString(),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new GetSupportRequestsResult(items, total);
    }
}
