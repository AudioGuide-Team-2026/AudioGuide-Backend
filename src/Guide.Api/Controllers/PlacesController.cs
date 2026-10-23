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
    public async Task<ActionResult<IReadOnlyList<Place>>> GetAll(CancellationToken cancellationToken)
    {
        var places = await db.Places
            .AsNoTracking()
            .OrderBy(place => place.Name)
            .ToListAsync(cancellationToken);

        return Ok(places);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Place>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var place = await db.Places.AsNoTracking()
            .FirstOrDefaultAsync(place => place.Id == id, cancellationToken);

        return place is null ? NotFound() : Ok(place);
    }

    [HttpPost]
    public async Task<ActionResult<Place>> Create(
        Place place,
        CancellationToken cancellationToken)
    {
        place.Id = Guid.NewGuid();

        db.Places.Add(place);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = place.Id }, place);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Place>> Update(
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
        place.Description = updatedPlace.Description;
        place.Latitude = updatedPlace.Latitude;
        place.Longitude = updatedPlace.Longitude;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(place);
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
