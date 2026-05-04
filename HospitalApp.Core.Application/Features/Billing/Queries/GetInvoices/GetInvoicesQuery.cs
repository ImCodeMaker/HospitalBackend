using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Queries.GetInvoices;

public record GetInvoicesQuery(
    Guid? PatientId = null,
    string? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedResult<InvoiceDto>>>;
