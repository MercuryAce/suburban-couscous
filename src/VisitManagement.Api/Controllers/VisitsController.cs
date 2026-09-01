using Microsoft.AspNetCore.Mvc;
using VisitManagement.Application.DTOs;
using VisitManagement.Application.Usecases;

namespace VisitManagement.Api.Controllers;

[ApiController]
[Route("api/v1/visits")]
public class VisitsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VisitResponse>> Create(
        [FromBody] CreateVisitRequest request,
        CreateVisit createVisit,
        CancellationToken ct)
    {
        var visit = await createVisit.ExecuteAsync(request, createdBy: "local-dev", ct);
        return CreatedAtAction(nameof(GetById), new { id = visit.Id }, visit);
    }

    [HttpGet]
    public async Task<ActionResult<PagedVisitsResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        GetAllVisits getAllVisits = default!,
        CancellationToken ct = default)
    {
        var visits = await getAllVisits.ExecuteAsync(
            new GetAllVisitsRequest(page, pageSize), ct);
        return Ok(visits);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitResponse>> GetById(
        Guid id,
        GetVisitById getVisitById,
        CancellationToken ct)
    {
        var visit = await getVisitById.ExecuteAsync(id, ct);
        if (visit is null)
        {
            return NotFound();
        }
        return Ok(visit);
    }
}