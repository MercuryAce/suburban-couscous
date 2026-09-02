using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitManagement.Api.Auth;
using VisitManagement.Application.DTOs;
using VisitManagement.Application.Usecases;

namespace VisitManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/visits")]
public sealed class VisitsController(
    CreateVisit createVisit,
    GetAllVisits getAllVisits,
    GetVisitById getVisitById,
    UpdateVisit updateVisit) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = ScopeClaims.WritePolicy)]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VisitResponse>> Create(
        [FromBody] CreateVisitRequest request,
        CancellationToken ct)
    {
        var visit = await createVisit.ExecuteAsync(request, Actor(), ct);
        return CreatedAtAction(nameof(GetById), new { id = visit.Id }, visit);
    }

    [HttpGet]
    [Authorize(Policy = ScopeClaims.ReadPolicy)]
    public async Task<ActionResult<PagedVisitsResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var visits = await getAllVisits.ExecuteAsync(
            new GetAllVisitsRequest(page, pageSize), ct);
        return Ok(visits);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ScopeClaims.ReadPolicy)]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var visit = await getVisitById.ExecuteAsync(id, ct);
        if (visit is null)
        {
            return NotFound();
        }
        return Ok(visit);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ScopeClaims.WritePolicy)]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VisitResponse>> Update(
        Guid id,
        [FromBody] CreateVisitRequest request,
        CancellationToken ct)
    {
        var updated = await updateVisit.ExecuteAsync(id, request, Actor(), ct);
        if (updated is null)
            return NotFound();
        return Ok(updated);
    }

    private string Actor() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException();
}
