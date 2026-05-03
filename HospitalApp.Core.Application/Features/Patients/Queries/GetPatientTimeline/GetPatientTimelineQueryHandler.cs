using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.GetPatientTimeline;

public class GetPatientTimelineQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetPatientTimelineQuery, Result<List<PatientTimelineItemDto>>>
{
    public async Task<Result<List<PatientTimelineItemDto>>> Handle(GetPatientTimelineQuery query, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(query.PatientId, ct);
        if (patient is null)
            return Result<List<PatientTimelineItemDto>>.NotFound("Patient not found.");

        var items = new List<PatientTimelineItemDto>();
        var category = query.Category;

        // Consults
        if (category is null || category == "Consult")
        {
            var consults = await uow.Consults.FindAsync(c => c.PatientId == query.PatientId, ct);
            items.AddRange(consults.Select(c => new PatientTimelineItemDto
            {
                Category = "Consult",
                EntityId = c.Id,
                Title = c.Status.ToString(),
                Detail = c.ChiefComplaint,
                OccurredAt = c.CreatedAt,
            }));
        }

        // Payments — represented via Invoices for the patient
        if (category is null || category == "Payment")
        {
            var invoices = await uow.Invoices.FindAsync(i => i.PatientId == query.PatientId, ct);
            items.AddRange(invoices.Select(i => new PatientTimelineItemDto
            {
                Category = "Payment",
                EntityId = i.Id,
                Title = $"Invoice {i.InvoiceNumber}",
                Detail = i.Status.ToString(),
                OccurredAt = i.CreatedAt,
            }));
        }

        // LabOrders
        if (category is null || category == "LabOrder")
        {
            var consultIds = (await uow.Consults.FindAsync(c => c.PatientId == query.PatientId, ct))
                .Select(c => c.Id)
                .ToHashSet();

            if (consultIds.Count > 0)
            {
                var labOrders = await uow.LabOrders.FindAsync(l => consultIds.Contains(l.ConsultId), ct);
                items.AddRange(labOrders.Select(l => new PatientTimelineItemDto
                {
                    Category = "LabOrder",
                    EntityId = l.Id,
                    Title = l.TestName,
                    Detail = l.Status,
                    OccurredAt = l.CreatedAt,
                }));
            }
        }

        // Appointments
        if (category is null || category == "Appointment")
        {
            var appointments = await uow.Appointments.FindAsync(a => a.PatientId == query.PatientId, ct);
            items.AddRange(appointments.Select(a => new PatientTimelineItemDto
            {
                Category = "Appointment",
                EntityId = a.Id,
                Title = $"{a.Type} — {a.Status}",
                Detail = a.Reason,
                OccurredAt = a.ScheduledDate,
            }));
        }

        // Apply date range filters
        if (query.From.HasValue)
            items = items.Where(i => i.OccurredAt >= query.From.Value).ToList();

        if (query.To.HasValue)
            items = items.Where(i => i.OccurredAt <= query.To.Value).ToList();

        // Sort descending chronologically
        items = items.OrderByDescending(i => i.OccurredAt).ToList();

        return Result<List<PatientTimelineItemDto>>.Success(items);
    }
}
