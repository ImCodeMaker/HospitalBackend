using FluentValidation;

namespace HospitalApp.Core.Application.Features.Billing.Commands.ProcessPayment;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");
    }
}
