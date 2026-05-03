using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.HR.DTOs;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.HR.Queries.GetEmployeePerformance;

public class GetEmployeePerformanceQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetEmployeePerformanceQuery, Result<EmployeePerformanceDto>>
{
    public async Task<Result<EmployeePerformanceDto>> Handle(GetEmployeePerformanceQuery query, CancellationToken ct)
    {
        var employee = await uow.Employees.GetByIdAsync(query.EmployeeId, ct);
        if (employee is null)
            return Result<EmployeePerformanceDto>.NotFound("Employee not found.");

        var from = query.From ?? DateTime.UtcNow.AddDays(-30);
        var to = query.To ?? DateTime.UtcNow;

        var consults = await uow.Consults.FindAsync(
            c => c.DoctorId == query.EmployeeId
                 && c.Status == ConsultStatusEnum.Finished
                 && c.FinishedAt >= from && c.FinishedAt <= to, ct);

        var prescriptions = await uow.Prescriptions.CountAsync(
            p => p.PrescribedByDoctorId == query.EmployeeId
                 && p.CreatedAt >= from && p.CreatedAt <= to, ct);

        var labOrders = await uow.LabOrders.CountAsync(
            l => l.OrderedByDoctorId == query.EmployeeId
                 && l.CreatedAt >= from && l.CreatedAt <= to, ct);

        var consultIds = consults.Select(c => c.Id).ToHashSet();
        var invoices = await uow.Invoices.FindAsync(
            i => consultIds.Contains(i.ConsultId), ct);
        var revenue = invoices.Sum(i => i.PaidAmount);

        return Result<EmployeePerformanceDto>.Success(new EmployeePerformanceDto
        {
            EmployeeId = query.EmployeeId,
            FullName = $"{employee.FirstName} {employee.LastName}",
            PatientsAttended = consults.Count,
            PrescriptionsWritten = prescriptions,
            LabOrdersPlaced = labOrders,
            RevenueGenerated = revenue,
        });
    }
}
