using MediatR;
using StockApp.Entities;

namespace StockApp.App.SupportRequest.Command;

public record CreateSupportRequestCommand(
    string Email,
    string Subject,
    string? DetectedIntent) : IRequest<CreateSupportRequestResult>;

public record CreateSupportRequestResult(int Id, string Email, string Subject);

internal class CreateSupportRequestCommandHandler : IRequestHandler<CreateSupportRequestCommand, CreateSupportRequestResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateSupportRequestCommandHandler> _logger;

    public CreateSupportRequestCommandHandler(ApplicationDbContext context, ILogger<CreateSupportRequestCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateSupportRequestResult> Handle(CreateSupportRequestCommand request, CancellationToken cancellationToken)
    {
        var supportRequest = new Entities.SupportRequest
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            Subject = request.Subject.Trim(),
            DetectedIntent = request.DetectedIntent,
            CreatedAt = DateTime.UtcNow
        };

        _context.SupportRequests.Add(supportRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support request created. Id={Id}, Email={Email}", supportRequest.Id, supportRequest.Email);

        return new CreateSupportRequestResult(supportRequest.Id, supportRequest.Email, supportRequest.Subject);
    }
}
