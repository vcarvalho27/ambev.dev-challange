using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Sample "webhook" event publisher. Instead of pushing the event to a message broker or an
/// external HTTP endpoint, it logs what would have been sent, standing in for the real dispatch.
/// </summary>
public class LogEventPublisher : IEventPublisher
{
    private readonly ILogger<LogEventPublisher> _logger;

    public LogEventPublisher(ILogger<LogEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        var eventName = typeof(TEvent).Name;
        var payload = JsonSerializer.Serialize(@event);

        _logger.LogInformation(
            "[Webhook] event={EventName} payload={Payload}",
            eventName, payload);

        return Task.CompletedTask;
    }
}
