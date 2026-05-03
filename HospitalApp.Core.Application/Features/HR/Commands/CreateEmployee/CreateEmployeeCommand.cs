using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.HR.Commands.CreateEmployee;

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string NationalId,
    EmployeeRoleEnum Role,
    Guid? SpecialtyId,
    string? MedicalLicenseNumber,
    DateTime StartDate,
    EmploymentTypeEnum EmploymentType,
    string? Department,
    decimal Salary,
    PayFrequencyEnum PayFrequency,
    string? BankAccount,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    Guid? UserId
);

public record CreateEmployeeCommand(CreateEmployeeRequest Request) : IRequest<Result<Guid>>;
