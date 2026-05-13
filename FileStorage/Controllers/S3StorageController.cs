using FileStorage.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace FileStorage.Controllers;

/// <summary>
/// Контроллер для взаимодействия с S3
/// </summary>
/// <param name="s3Service"></param>
/// <param name="logger"></param>
[ApiController]
[Route("api/s3")]
public class S3StorageController(IS3Service s3Service, ILogger<S3StorageController> logger) : ControllerBase
{
    /// <summary>
    /// Получение списка хранящихся в S3 файлов
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<string>>> ListFiles()
    {
        try
        {
            var list = await s3Service.GetFileList();
            logger.LogInformation("Listed {Count} files from bucket", list.Count);
            return Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing files");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Получает строковое представление хранящегося в S3 файла
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("{key}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonNode>> GetFile(string key)
    {
        try
        {
            var node = await s3Service.DownloadFile(key);
            return Ok(node);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading file {Key}", key);
            return BadRequest(ex.Message);
        }
    }
}