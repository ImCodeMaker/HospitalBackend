using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.UpdateTemplate;

public record UpdateConsultFieldTemplateCommand(
    Guid TemplateId,
    UpdateConsultFieldTemplateRequest Request) : IRequest<Result<Guid>>;
