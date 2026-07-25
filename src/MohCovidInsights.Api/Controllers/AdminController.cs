using MohCovidInsights.Infrastructure.Sync;
using MohCovidInsights.Infrastructure.Sync.Interfaces;

namespace MohCovidInsights.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class AdminController(IDatasetSyncService sync) : ControllerBase
{
    /// <summary>Manually trigger ingestion of all configured MOH datasets.</summary>
    [HttpPost("sync")]
    [ProducesResponseType<SyncSummary>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SyncSummary>> Sync(CancellationToken ct) =>
        Ok(await sync.SyncAllAsync(ct));
}