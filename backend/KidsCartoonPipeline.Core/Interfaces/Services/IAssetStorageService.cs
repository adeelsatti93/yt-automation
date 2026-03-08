namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IAssetStorageService
{
    Task<string> SaveFileAsync(byte[] data, string relativePath);
    Task<Stream> GetFileStreamAsync(string relativePath);
    Task<bool> FileExistsAsync(string relativePath);
    Task DeleteFileAsync(string relativePath);
    Task DeleteDirectoryAsync(string relativePath);
    string GetFullPath(string relativePath);
}
