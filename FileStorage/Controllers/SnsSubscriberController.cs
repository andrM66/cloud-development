using Amazon.SimpleNotificationService.Util;
using FileStorage.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FileStorage.Controllers;

/// <summary>
/// Контроллер для сообщений от SNS
/// </summary>
/// <param name="s3Service"></param>
/// <param name="logger"></param>
[ApiController]
[Route("api/sns")]
public class SnsSubscriberController(IS3Service s3Service, ILogger<SnsSubscriberController> logger) : ControllerBase
{
    /// <summary>
    /// Веб-хук, для получения уведомлений и подтверждения подписки.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ReceiveMessage()
    {
        logger.LogInformation("SNS webhook was called");
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var jsonContent = await reader.ReadToEndAsync();
            var snsMessage = Message.ParseMessage(jsonContent);

            if (snsMessage.Type == "SubscriptionConfirmation")
            {
                logger.LogInformation("SubscriptionConfirmation received");
                using var httpClient = new HttpClient();
                var builder = new UriBuilder(new Uri(snsMessage.SubscribeURL))
                {
                    Scheme = "http",
                    Host = "localhost",
                    Port = 4566
                };
                var response = await httpClient.GetAsync(builder.Uri);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception($"SubscriptionConfirmation returned {response.StatusCode}: {body}");
                }
                logger.LogInformation("Subscription confirmed");
                return Ok();
            }

            if (snsMessage.Type == "Notification")
            {
                await s3Service.UploadFile(snsMessage.MessageText);
                logger.LogInformation("Notification processed and saved to S3");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing SNS notification");
        }

        return Ok();
    }
}