using FluentValidation;
using VisitManagement.Application.DTOs;

namespace VisitManagement.Application.Validators;

public sealed class GetAllVisitsRequestValidator : AbstractValidator<GetAllVisitsRequest>
{
    public GetAllVisitsRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
