using FluentValidation;
using HospitalApp.Core.Application.Features.Patients.DTOs;

namespace HospitalApp.Core.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Request.DocumentNumber)
            .NotEmpty().WithMessage("Document number is required.")
            .MaximumLength(50);

        RuleFor(x => x.Request.Nationality)
            .NotEmpty().WithMessage("Nationality is required.")
            .MaximumLength(100);

        RuleFor(x => x.Request.HomeAddress)
            .NotEmpty().WithMessage("Home address is required.")
            .MaximumLength(500);

        RuleFor(x => x.Request.BirthDate)
            .NotEmpty().WithMessage("Birth date is required.")
            .LessThan(DateTime.UtcNow).WithMessage("Birth date must be in the past.");

        RuleFor(x => x.Request.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.Email))
            .WithMessage("Invalid email format.");

        // Guardian required for minors
        When(x => (DateTime.UtcNow.Year - x.Request.BirthDate.Year) < 18, () =>
        {
            RuleFor(x => x.Request.GuardianFirstName)
                .NotEmpty().WithMessage("Guardian first name is required for minors.");
            RuleFor(x => x.Request.GuardianLastName)
                .NotEmpty().WithMessage("Guardian last name is required for minors.");
            RuleFor(x => x.Request.GuardianDocumentType)
                .NotNull().WithMessage("Guardian document type is required for minors.");
            RuleFor(x => x.Request.GuardianDocumentNumber)
                .NotEmpty().WithMessage("Guardian document number is required for minors.");
            RuleFor(x => x.Request.GuardianPhone)
                .NotEmpty().WithMessage("Guardian phone is required for minors.");
            RuleFor(x => x.Request.GuardianRelationship)
                .NotNull().WithMessage("Guardian relationship is required for minors.");
        });

        // Insurance validation
        When(x => x.Request.HasInsurance, () =>
        {
            RuleFor(x => x.Request.InsuranceCompanyId)
                .NotNull().WithMessage("Insurance company is required when patient has insurance.");
            RuleFor(x => x.Request.InsurancePolicyNumber)
                .NotEmpty().WithMessage("Insurance policy number is required.");
            RuleFor(x => x.Request.InsuranceCoveragePercentage)
                .InclusiveBetween(0, 100).WithMessage("Coverage percentage must be between 0 and 100.");
        });
    }
}
