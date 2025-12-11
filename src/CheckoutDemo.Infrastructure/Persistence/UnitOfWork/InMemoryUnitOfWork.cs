using CheckoutDemo.Application.Common.Abstractions;

namespace CheckoutDemo.Infrastructure.Persistence.UnitOfWork
{
    public sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
