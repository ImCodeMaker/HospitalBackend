using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.AttachConsultImage;

public record AttachConsultImageCommand(AttachConsultImageRequest Request, Guid UploadedByUserId)
    : IRequest<Result<Guid>>;
