using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Queries.GetTemplatesBySpecialty;

public record GetTemplatesBySpecialtyQuery(Guid SpecialtyId) : IRequest<Result<List<ConsultFieldTemplateDto>>>;
