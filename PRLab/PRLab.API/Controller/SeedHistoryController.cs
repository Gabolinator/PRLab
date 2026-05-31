using Microsoft.AspNetCore.Mvc;
using PRLab.Application.Interface.DB.Repositories;

namespace PRLab.API.Controller;

[ApiController]
[Route("seed-history")]
public class SeedHistoryController(ISeedHistoryRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllSeedHistory(CancellationToken ct = default)
    {
        try
        {
            var history = await repository.ListAsync(ct);
            return Ok(history);
        }

        catch (Exception e)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {e.GetBaseException().Message}");
        }
    }
}