using Guide.Api.Models;

namespace Guide.Api.Contracts;

public sealed record PlaceResponse(
    Guid Id,
    string Address,
    LocationResponse Location,
    string Name,
    string Description)
{
    public static PlaceResponse FromPlace(Place place) => new(
        place.Id,
        place.Address,
        new LocationResponse(place.Latitude, place.Longitude),
        place.Name,
        place.Description ?? string.Empty);
}

public sealed record LocationResponse(decimal Latitude, decimal Longitude);
