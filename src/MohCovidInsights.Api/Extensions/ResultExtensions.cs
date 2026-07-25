using MohCovidInsights.Domain.Common;

namespace MohCovidInsights.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller) =>
        result.IsSuccess
            ? controller.Ok(result.Value)
            : ToProblem(result.Error, controller);

    private static ActionResult ToProblem(Error error, ControllerBase controller)
    {
        var (status, title) = error.Code switch
        {
            "validation" => (StatusCodes.Status400BadRequest, "Invalid request"),
            "not_found" => (StatusCodes.Status404NotFound, "No data available"),
            "upstream" => (StatusCodes.Status502BadGateway, "Upstream data source unavailable"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };

        return controller.Problem(detail: error.Message, statusCode: status, title: title);
    }
}