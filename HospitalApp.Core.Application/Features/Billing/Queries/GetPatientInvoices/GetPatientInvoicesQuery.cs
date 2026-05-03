using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Queries.GetPatientInvoices;

public record GetPatientInvoicesQuery(Guid PatientId, InvoiceStatusEnum? Status, int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<PaginatedResult<InvoiceDto>>>;
