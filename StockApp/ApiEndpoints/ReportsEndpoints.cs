using MediatR;
using StockApp.App.Product.Query;
using StockApp.App.Reports.Query;
using StockApp.Services;

namespace StockApp.ApiEndpoints;

public static class ReportsEndpoints
{
    public static void MapReports(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");
        
        group.MapGet("/critical-stock/pdf", async (
            IMediator mediator,
            IPdfService pdfService,
            CancellationToken cancellationToken) =>
        {
            var products = await mediator.Send(new GetCriticalStockProductsQuery(), cancellationToken);
            var pdfBytes = await pdfService.GenerateCriticalStockPdf(products);
            return Results.File(pdfBytes, "application/pdf", "critical-stock-report.pdf");
        })
        .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf");

        group.MapPost("/natural-language", async (
            NaturalLanguageReportRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetNaturalLanguageReportQuery(request.Question ?? string.Empty), cancellationToken);
            return Results.Ok(result);
        })
        .Produces<NaturalLanguageReportResponse>(StatusCodes.Status200OK);
    }
}

public record NaturalLanguageReportRequest(string Question);

