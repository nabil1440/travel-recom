namespace AppCore.Models;

public sealed record RankedDistrict(
    int DistrictId,
    string DistrictName,
    double Temp2Pm,
    double Pm25_2Pm,
    int Rank
);