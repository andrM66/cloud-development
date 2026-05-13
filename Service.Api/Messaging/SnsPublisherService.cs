using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Service.Api.Entities;
using System.Net;
using System.Text.Json;

namespace Service.Api.Messaging;

/// <summary>
/// Служба для отправки сообщений в SNS
/// </summary>
/// <param name="client"></param>
/// <param name="configuration"></param>
/// <param name="logger"></param>
public class SnsPublisherService(
    IAmazonSimpleNotificationService client,
    IConfiguration configuration,
    ILogger<SnsPublisherService> logger) : IProducerService
{
    private readonly string _topicArn = configuration["AWS:Resources:SNSTopicArn"]
        ?? throw new KeyNotFoundException("SNS topic ARN was not found in configuration");

    /// <inheritdoc/>
    public async Task SendMessage(StudyCourse course)
    {
        try
        {
            var json = JsonSerializer.Serialize(course);
            var request = new PublishRequest
            {
                Message = json,
                TopicArn = _topicArn
            };
            var response = await client.PublishAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK)
                logger.LogInformation("Course {Id} sent to SNS", course.Id);
            else
                throw new Exception($"SNS returned {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send course {Id} through SNS", course.Id);
        }
    }
}