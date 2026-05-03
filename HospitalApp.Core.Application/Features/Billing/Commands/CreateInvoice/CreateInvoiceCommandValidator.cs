using FluentValidation;
using HospitalApp.Core.Application.Features.Billing.DTOs;

namespace HospitalApp.Core.Application.Features.Billing.Commands.CreateInvoice;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Request.ConsultId)
            .NotEmpty().WithMessage("Consult ID is required.");

        RuleFor(x => x.Request.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount must be 0 or greater.");

        RuleFor(x => x.Request.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be 0 or greater.");

        RuleFor(x => x.Request.LineItems)
            .NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Request.LineItems).SetValidator(new CreateLineItemRequestValidator());
    }
}

public class CreateLineItemRequestValidator : AbstractValidator<CreateLineItemRequest>
{
    public CreateLineItemRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Line item description is required.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be 0 or greater.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 9999).WithMessage("Quantity must be between 1 and 9999.");
    }
}
