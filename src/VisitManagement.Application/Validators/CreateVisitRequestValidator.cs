using FluentValidation;
using VisitManagement.Application.DTOs;

namespace VisitManagement.Application.Validators;

public class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.VehicleLicenceNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Visitor).NotNull();
        RuleFor(x => x.Visitor.Id).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Visitor.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Visitor.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Activities).NotEmpty();
        RuleForEach(x => x.Activities).ChildRules(a => 
        {
            a.RuleFor(x => x.TravellerNumber).NotEmpty().MaximumLength(64);
            a.RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt);
        });
    }
}