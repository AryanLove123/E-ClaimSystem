namespace EClaim.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken ct = default);
    Task<Stream> GetFileAsync(string storedFileName, CancellationToken ct = default);
    bool IsAllowedFile(string fileName, string contentType, long fileSizeBytes);
}
