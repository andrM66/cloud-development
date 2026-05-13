using Service.Api.Entities;

namespace Service.Api.Messaging;

/// <summary>
/// Интерфейс для отправки в брокер сообщений
/// </summary>
public interface IProducerService
{
    public Task SendMessage(StudyCourse course);
}
