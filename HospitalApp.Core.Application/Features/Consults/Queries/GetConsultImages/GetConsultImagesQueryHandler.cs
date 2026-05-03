using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetConsultImages;

public class GetConsultImagesQueryHandler(IUnitOfWork uow, IFileStorageService fileStorage)
    : IRequestHandler<GetConsultImagesQuery, Result<List<ConsultImageDto>>>
{
    public async Task<Result<List<ConsultImageDto>>> Handle(GetConsultImagesQuery query, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(query.ConsultId, ct);
        if (consult is null)
            return Result<List<ConsultImageDto>>.NotFound("Consult not found.");

        var images = await uow.ConsultImages.FindAsync(i => i.ConsultId == query.ConsultId, ct);

        var dtos = images.Select(image => new ConsultImageDto
        {
            Id = image.Id,
            ConsultId = image.ConsultId,
            FilePath = image.FilePath,
            Url = fileStorage.GetUrl(image.FilePath),
            Description = image.Caption,
            CreatedAt = image.CreatedAt,
        }).ToList();

        return Result<List<ConsultImageDto>>.Success(dtos);
    }
}
