using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.HR.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateEmployeeCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var duplicate = await uow.Employees.ExistsAsync(e => e.NationalId == req.NationalId, ct);
        if (duplicate)
            return Result<Guid>.Failure("Employee with this national ID already exists.", 409);

        var employee = new Employee
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            NationalId = req.NationalId,
            Role = req.Role,
            SpecialtyId = req.SpecialtyId,
            MedicalLicenseNumber = req.MedicalLicenseNumber,
            StartDate = req.StartDate,
            EmploymentType = req.EmploymentType,
            Department = req.Department,
            Salary = req.Salary,
            PayFrequency = req.PayFrequency,
            BankAccount = req.BankAccount,
            EmergencyContactName = req.EmergencyContactName,
            EmergencyContactPhone = req.EmergencyContactPhone,
            UserId = req.UserId,
        };

        await uow.Employees.AddAsync(employee, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Created(employee.Id);
    }
}
