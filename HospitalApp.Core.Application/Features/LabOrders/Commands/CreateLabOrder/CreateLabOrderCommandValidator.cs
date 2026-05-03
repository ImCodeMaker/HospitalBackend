using FluentValidation;
using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.CreateLabOrder;

public class CreateLabOrderCommandValidator : AbstractValidator<CreateLabOrderCommand>
{
    public CreateLabOrderCommandValidator()
    {
        RuleFor(x => x.Request.ConsultId)
            .NotEmpty().WithMessage("Consult ID is required.");

        RuleFor(x => x.Request.TestName)
            .NotEmpty().WithMessage("Test name is required.")
            .MaximumLength(200).WithMessage("Test name must not exceed 200 characters.");

        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required.");
    }
}
