using FluentValidation;

namespace HospitalApp.Core.Application.Features.Prescriptions.Commands.AddPrescription;

public class AddPrescriptionCommandValidator : AbstractValidator<AddPrescriptionCommand>
{
    public AddPrescriptionCommandValidator()
    {
        RuleFor(x => x.ConsultId)
            .NotEmpty().WithMessage("Consult ID is required.");

        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(x => x.Request.DrugName)
            .NotEmpty().WithMessage("Drug name is required.")
            .MaximumLength(200).WithMessage("Drug name must not exceed 200 characters.");

        RuleFor(x => x.Request.Dosage)
            .NotEmpty().WithMessage("Dosage is required.");

        RuleFor(x => x.Request.Frequency)
            .NotEmpty().WithMessage("Frequency is required.");

        RuleFor(x => x.Request.RouteOfAdministration)
            .NotEmpty().WithMessage("Route of administration is required.");

        RuleFor(x => x.Request.DurationDays)
            .InclusiveBetween(1, 365).WithMessage("Duration must be between 1 and 365 days.");

        RuleFor(x => x.Request.QuantityToDispense)
            .InclusiveBetween(1, 9999).WithMessage("Quantity to dispense must be between 1 and 9999.");
    }
}
