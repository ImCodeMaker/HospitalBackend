using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalInvoices;

public record GetPortalInvoicesQuery(Guid PatientId) : IRequest<Result<List<PortalInvoiceDto>>>;
