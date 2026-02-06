namespace Infrastructure.Persistence.Seed;

using System.Text.Json.Serialization;

internal sealed class DistrictSeedDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("lat")]
    public string Latitude { get; set; } = null!;

    [JsonPropertyName("long")]
    public string Longitude { get; set; } = null!;
}

internal sealed class DistrictSeedRoot
{
    [JsonPropertyName("districts")]
    public List<DistrictSeedDto> Districts { get; set; } = [];
}