using FluentValidation;

namespace HospitalApp.Core.Application.Features.Specialties.Commands.CreateSpecialty;

public class CreateSpecialtyCommandValidator : AbstractValidator<CreateSpecialtyCommand>
{
    public CreateSpecialtyCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Specialty name is required.")
            .MaximumLength(150).WithMessage("Specialty name must not exceed 150 characters.");

        RuleFor(x => x.Request.Code)
            .NotEmpty().WithMessage("Specialty code is required.")
            .MaximumLength(20).WithMessage("Specialty code must not exceed 20 characters.");

        RuleFor(x => x.Request.DefaultConsultDurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("Default consult duration must be between 5 and 480 minutes.");
    }
}
