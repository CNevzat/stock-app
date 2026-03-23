using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Product.Query;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/reports")]
[Tags("Reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("critical-stock/pdf")]
    [Authorize(Policy = "CanViewReports")]
    public async Task<IActionResult> CriticalStockPdf([FromServices] IPdfService pdfService,
        CancellationToken cancellationToken)
    {
        var products = await _mediator.Send(new GetCriticalStockProductsQuery(), cancellationToken);
        var pdfBytes = await pdfService.GenerateCriticalStockPdf(products);
        return File(pdfBytes, "application/pdf", "critical-stock-report.pdf");
    }
}
