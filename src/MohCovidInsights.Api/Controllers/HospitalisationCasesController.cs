using Microsoft.AspNetCore.Mvc;
using MohCovidInsights.Api.Extensions;
using MohCovidInsights.Application.Abstractions;
using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Application.HospitalisationCases.Dtos;
using MohCovidInsights.Application.HospitalisationCases.Queries;

namespace MohCovidInsights.Api.Controllers;

[ApiController]
[Route("api/v1/hospitalisation-cases")]
[Produces("application/json")]
public sealed class HospitalisationCasesController(IQueryDispatcher dispatcher) : ControllerBase
{
    /// <summary>Average daily hospitalised and ICU cases by epi week, clinical status and age group.</summary>
    [HttpGet]
    [ProducesResponseType<HospitalisationCasesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HospitalisationCasesDto>> Get(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? clinicalStatus,
        [FromQuery] string? ageGroup,
        CancellationToken ct)
    {
        var query = new GetHospitalisationCasesQuery(
            from ?? "2023-W09", to ?? "2024-W08", clinicalStatus, ageGroup);

        var result = await dispatcher.SendAsync(query, ct);
        return result.ToActionResult(this);
    }
}