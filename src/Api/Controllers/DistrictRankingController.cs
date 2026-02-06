namespace Api.Controllers;

using Api.Contracts.Responses;
using AppCore.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/leaderboard")]
public sealed class DistrictRankingController : ControllerBase
{
    private readonly IDistrictRankingService _rankingService;

    public DistrictRankingController(IDistrictRankingService rankingService)
    {
        _rankingService = rankingService;
    }

    [HttpGet]
    public async Task<ActionResult<LeaderboardResponse>> GetTopDistricts(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0 || count > 64)
            return BadRequest("Count must be between 1 and 64.");

        try
        {
            var ranked = await _rankingService
                .GetTopDistrictsAsync(count, cancellationToken);

            var response = new LeaderboardResponse(
                ranked.Select(r => new LeaderboardDistrictResponse(
                    r.DistrictId,
                    r.DistrictName,
                    r.Temp2Pm,
                    r.Pm25_2Pm,
                    r.Rank
                )).ToList()
            );

            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            // Leaderboard not ready yet
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Leaderboard data is not ready yet. Please try again later.");
        }
    }
}