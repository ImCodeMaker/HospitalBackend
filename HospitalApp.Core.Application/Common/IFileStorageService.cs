namespace HospitalApp.Core.Application.Common;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
    string GetUrl(string filePath);
}
