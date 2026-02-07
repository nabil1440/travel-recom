namespace AppCore.Models;

public enum RecommendationReasonCode
{
    SameSourceAndDestination,
    InvalidSourceDistrict,
    InvalidDestinationDistrict,
    DateOutOfRange,
    InsufficientData,
    DestinationCoolerAndCleaner,
    DestinationHotter,
    DestinationMorePolluted,
    DestinationHotterAndMorePolluted
}