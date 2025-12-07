using CheckoutDemo.Domain.Common;

namespace CheckoutDemo.Application.Common.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
