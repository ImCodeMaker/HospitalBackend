using FluentValidation;
using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.Request.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.Request.AssignedDoctorId)
            .NotEmpty().WithMessage("Assigned doctor ID is required.");

        RuleFor(x => x.Request.ScheduledDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled date must be in the future.");

        RuleFor(x => x.Request.DurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("Duration must be between 5 and 480 minutes.");
    }
}
