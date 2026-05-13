using System.Text.Json.Nodes;

namespace FileStorage.Storage;

/// <summary>
/// Интерфейс службы упраления файлами в объектном хранилище
/// </summary>
public interface IS3Service
{
    public Task<bool> UploadFile(string fileData);
    public Task<List<string>> GetFileList();
    public Task<JsonNode> DownloadFile(string filePath);
    public Task EnsureBucketExists();
}