namespace Api.Controllers;

using Api.Contracts.Requests;
using Api.Contracts.Responses;
using AppCore.Abstractions.Services;
using AppCore.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/travel")]
public sealed class TravelRecommendationController : ControllerBase
{
    private readonly ITravelRecommendationService _service;

    public TravelRecommendationController(
        ITravelRecommendationService service)
    {
        _service = service;
    }

    [HttpPost("recommendation")]
    public async Task<ActionResult<TravelRecommendationResponseDto>> RecommendAsync(
        TravelRecommendationRequestDto request,
        CancellationToken cancellationToken)
    {
        // Basic shape/range validation (API responsibility)
        if (request.TravelDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest("Travel date must be in the future.");
        }

        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            return BadRequest("Destination district name is required.");
        }

        var result = await _service.RecommendAsync(
            new TravelRecommendationRequest(
                request.Latitude,
                request.Longitude,
                request.Destination.Trim(),
                request.TravelDate),
            cancellationToken);

        var response = new TravelRecommendationResponseDto(
            Recommendation: result.IsRecommended ? "Recommended" : "Not Recommended",
            Reason: MapReason(result));

        return Ok(response);
    }

    private static string MapReason(TravelRecommendationResult result)
        => result.ReasonCode switch
        {
            RecommendationReasonCode.SameSourceAndDestination =>
                "You are already in the destination district.",

            RecommendationReasonCode.InvalidSourceDistrict =>
                "Your current location could not be matched to a district.",

            RecommendationReasonCode.InvalidDestinationDistrict =>
                "The selected destination district is invalid.",

            RecommendationReasonCode.DateOutOfRange =>
                "Forecast data is only available for the next 7 days.",

            RecommendationReasonCode.InsufficientData =>
                "Insufficient forecast data to make a recommendation.",

            RecommendationReasonCode.DestinationCoolerAndCleaner =>
                $"Your destination is {Math.Abs(result.TempDelta):0.#}°C cooler and has significantly better air quality. Enjoy your trip!",

            RecommendationReasonCode.DestinationHotterAndMorePolluted =>
                "Your destination is hotter and has worse air quality than your current location. It's better to stay where you are.",

            RecommendationReasonCode.DestinationHotter =>
                "Your destination is hotter than your current location. It's better to stay where you are.",

            RecommendationReasonCode.DestinationMorePolluted =>
                "Your destination has worse air quality than your current location. It's better to stay where you are.",

            _ => "Unable to determine travel recommendation."
        };
}