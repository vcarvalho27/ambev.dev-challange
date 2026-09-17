using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Publishes domain/integration events to external subscribers.
/// In a production scenario this would push to a message broker (e.g. RabbitMQ, SQS, Kafka);
/// implementations here may instead simulate delivery (e.g. via logging) as a webhook sample.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}
