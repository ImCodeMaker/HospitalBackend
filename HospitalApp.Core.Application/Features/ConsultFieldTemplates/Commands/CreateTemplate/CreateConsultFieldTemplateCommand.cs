using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.CreateTemplate;

public record CreateConsultFieldTemplateCommand(
    CreateConsultFieldTemplateRequest Request,
    Guid CreatedByUserId) : IRequest<Result<Guid>>;
