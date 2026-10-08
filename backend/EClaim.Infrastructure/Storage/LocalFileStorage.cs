using EClaim.Application.Interfaces;
using Microsoft.Extensions.Configuration;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase) { "application/pdf", "image/jpeg", "image/png" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configurePath = configuration["Storage:BasePath"] ?? "Downloads/uploads";
        _basePath = Path.IsPathRooted(configurePath) ? configurePath : Path.Combine(AppContext.BaseDirectory, configurePath);

        Directory.CreateDirectory(_basePath);
    }

    public bool IsAllowedFile(string fileName, string contentType, long fileSizeBytes)
    {
        var ext = Path.GetExtension(fileName);
        return AllowedExtensions.Contains(ext)
            && AllowedContentTypes.Contains(contentType)
            && fileSizeBytes > 0
            && fileSizeBytes <= MaxFileSizeBytes;
    }
    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken ct = default)
    {
        var safeExtension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{safeExtension}";
        var fullPath = Path.Combine(_basePath, storedFileName);

        await using var fileOut = File.Create(fullPath);
        await fileStream.CopyToAsync(fileOut, ct);

        return storedFileName;
    }
    public Task<Stream> GetFileAsync(string storedFileName, CancellationToken ct = default)
    {
        if(storedFileName.Contains("..") || Path.IsPathRooted(storedFileName) || storedFileName.Contains('/') || storedFileName.Contains('\\'))
        {
            throw new UnauthorizedAccessException("Invalid file reference");
        }

        var fullPath = Path.Combine(_basePath, storedFileName);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The requested file was not found.", storedFileName);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }
}