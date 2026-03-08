using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace KidsCartoonPipeline.Infrastructure.Services.Storage;

public class LocalAssetStorageService : IAssetStorageService
{
    private readonly string _basePath;

    public LocalAssetStorageService(IConfiguration config)
    {
        _basePath = config["Storage:BasePath"] ?? "./storage";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(byte[] data, string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null)
        {
         var tt = Directory.CreateDirectory(directory);
        }
        await File.WriteAllBytesAsync(fullPath, data);
        return relativePath;
    }

    public Task<Stream> GetFileStreamAsync(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Asset not found: {relativePath}");
        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task<bool> FileExistsAsync(string relativePath)
        => Task.FromResult(File.Exists(GetFullPath(relativePath)));

    public Task DeleteFileAsync(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task DeleteDirectoryAsync(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, recursive: true);
        return Task.CompletedTask;
    }

    public string GetFullPath(string relativePath)
        => Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
}
