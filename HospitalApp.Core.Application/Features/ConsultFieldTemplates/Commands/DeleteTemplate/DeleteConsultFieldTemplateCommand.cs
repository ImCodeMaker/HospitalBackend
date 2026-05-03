using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.DeleteTemplate;

public record DeleteConsultFieldTemplateCommand(Guid TemplateId) : IRequest<Result>;
