namespace AppCore.Models;

public sealed record District(
    int Id,
    string Name,
    double Latitude,
    double Longitude
);