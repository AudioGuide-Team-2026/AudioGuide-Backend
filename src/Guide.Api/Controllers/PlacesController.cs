using Guide.Api.Contracts;
using Guide.Api.Data;
using Guide.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guide.Api.Controllers;

[ApiController]
[Route("api/v1/places")]
public sealed class PlacesController(GuideDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlaceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var places = await db.Places
            .AsNoTracking()
            .OrderBy(place => place.Name)
            .ToListAsync(cancellationToken);

        return Ok(places.Select(PlaceResponse.FromPlace).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var place = await db.Places.AsNoTracking()
            .FirstOrDefaultAsync(place => place.Id == id, cancellationToken);

        return place is null ? NotFound() : Ok(PlaceResponse.FromPlace(place));
    }

    [HttpPost]
    public async Task<ActionResult<PlaceResponse>> Create(
        Place place,
        CancellationToken cancellationToken)
    {
        place.Id = Guid.NewGuid();

        db.Places.Add(place);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = place.Id }, PlaceResponse.FromPlace(place));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlaceResponse>> Update(
        Guid id,
        Place updatedPlace,
        CancellationToken cancellationToken)
    {
        var place = await db.Places.FirstOrDefaultAsync(place => place.Id == id, cancellationToken);

        if (place is null)
        {
            return NotFound();
        }

        place.Name = updatedPlace.Name;
        place.Address = updatedPlace.Address;
        place.Description = updatedPlace.Description;
        place.Latitude = updatedPlace.Latitude;
        place.Longitude = updatedPlace.Longitude;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(PlaceResponse.FromPlace(place));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var place = await db.Places.FirstOrDefaultAsync(place => place.Id == id, cancellationToken);

        if (place is null)
        {
            return NotFound();
        }

        db.Places.Remove(place);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
