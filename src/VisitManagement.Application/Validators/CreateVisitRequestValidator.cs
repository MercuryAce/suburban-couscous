using FluentValidation;
using VisitManagement.Application.DTOs;

namespace VisitManagement.Application.Validators;

public class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.VehicleLicenceNumber).NotEmpty();
        RuleFor(x => x.Visitor).NotNull();
        RuleFor(x => x.Visitor.Id).NotEmpty();
        RuleFor(x => x.Visitor.FirstName).NotEmpty();
        RuleFor(x => x.Visitor.LastName).NotEmpty();
        RuleFor(x => x.Activities).NotEmpty();
        RuleForEach(x => x.Activities).ChildRules(a => 
        {
            a.RuleFor(x => x.TravellerNumber).NotEmpty();
            a.RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt);
        });
    }
}