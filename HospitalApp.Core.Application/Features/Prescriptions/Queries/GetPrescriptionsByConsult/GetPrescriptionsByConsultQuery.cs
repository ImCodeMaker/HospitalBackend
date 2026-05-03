using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Prescriptions.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Prescriptions.Queries.GetPrescriptionsByConsult;

public record GetPrescriptionsByConsultQuery(Guid ConsultId) : IRequest<Result<List<PrescriptionDto>>>;
