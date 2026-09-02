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
    /// <summary>
    /// Create a visit. Requires scope visits:write.
    /// </summary>
    /// <param name="request">The visit to create.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created visit.</returns>
    [HttpPost]
    [Authorize(Policy = ScopeClaims.WritePolicy)]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<VisitResponse>> Create(
        [FromBody] CreateVisitRequest request,
        CancellationToken ct)
    {
        var visit = await createVisit.ExecuteAsync(request, Actor(), ct);
        return CreatedAtAction(nameof(GetById), new { id = visit.Id }, visit);
    }

    /// <summary>
    /// Paged list of visits, newest first. Requires scope visits:read.
    /// </summary>
    /// <param name="page">1-based page index. Default 1. Must be ≥ 1.</param>
    /// <param name="pageSize">Items per page. Default 50. Must be 1–100.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The visits.</returns>
    [HttpGet]
    [Authorize(Policy = ScopeClaims.ReadPolicy)]
    [ProducesResponseType(typeof(PagedVisitsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedVisitsResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var visits = await getAllVisits.ExecuteAsync(
            new GetAllVisitsRequest(page, pageSize), ct);
        return Ok(visits);
    }

    /// <summary>
    /// Get a visit by ID. Requires scope visits:read.
    /// </summary>
    /// <param name="id">The ID of the visit.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The visit.</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = ScopeClaims.ReadPolicy)]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<VisitResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var visit = await getVisitById.ExecuteAsync(id, ct);
        if (visit is null)
            return VisitNotFound();
        return Ok(visit);
    }

    /// <summary>
    /// Update a visit. Requires scope visits:write.
    /// </summary>
    /// <param name="id">The ID of the visit.</param>
    /// <param name="request">The visit to update.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated visit.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = ScopeClaims.WritePolicy)]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<VisitResponse>> Update(
        Guid id,
        [FromBody] CreateVisitRequest request,
        CancellationToken ct)
    {
        var updated = await updateVisit.ExecuteAsync(id, request, Actor(), ct);
        if (updated is null)
            return VisitNotFound();
        return Ok(updated);
    }

    private string Actor() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException();

    private static ActionResult VisitNotFound() =>
        new NotFoundObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = "Visit not found."
        });
}
