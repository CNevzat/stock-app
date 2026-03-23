using Microsoft.AspNetCore.Mvc;
using StockApp.App.Elasticsearch;

namespace StockApp.Extensions;

public static class ElasticsearchOperationResultExtensions
{
    public static IActionResult ToActionResult(this ElasticsearchOperationResult r)
    {
        if (r.Success)
        {
            return new OkObjectResult(r.Data);
        }

        if (r.HttpStatus == 400 && !r.UseProblemDetails && r.ErrorTitle == null)
        {
            return new BadRequestObjectResult(r.ErrorMessage);
        }

        return new ObjectResult(new ProblemDetails
        {
            Title = r.ErrorTitle ?? "Error",
            Detail = r.ErrorMessage,
            Status = r.HttpStatus
        })
        {
            StatusCode = r.HttpStatus
        };
    }
}
