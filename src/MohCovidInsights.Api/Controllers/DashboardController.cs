using MohCovidInsights.Api.Extensions;
using MohCovidInsights.Api.Models;
using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Application.Dashboard.Dtos;
using MohCovidInsights.Application.Dashboard.Queries;

namespace MohCovidInsights.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class DashboardController(IQueryDispatcher dispatcher) : ControllerBase
{
    /// <summary>Composed dashboard payload for a given epi-week range.</summary>
    [HttpGet]
    [ProducesResponseType<DashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardDto>> Get(
        [FromQuery] DashboardRequest request,
        CancellationToken ct)
    {
        var query = new GetDashboardQuery(
            request.From, request.To, request.AgeGroup, request.ClinicalStatus);

        var result = await dispatcher.SendAsync(query, ct);
        return result.ToActionResult(this);
    }
}