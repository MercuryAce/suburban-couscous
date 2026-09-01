namespace VisitManagement.Application.DTOs;

public sealed record GetAllVisitsRequest(
    int Page = 1,
    int PageSize = 50);