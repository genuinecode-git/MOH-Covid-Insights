using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Api.Controllers;

public sealed record MetricDto(string Code, string DisplayName, string Unit);

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class MetricsController : ControllerBase
{
    /// <summary>Catalogue of metrics derived from the MOH datasets.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<MetricDto>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<MetricDto>> Get() =>
        Ok(MetricCode.All.Select(m => new MetricDto(m.Value, m.DisplayName, m.Unit)));
}