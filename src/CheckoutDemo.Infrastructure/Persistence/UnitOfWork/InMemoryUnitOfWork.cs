using CheckoutDemo.Application.Common.Abstractions;

namespace CheckoutDemo.Infrastructure.Persistence.UnitOfWork
{
    public sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        // 对 InMemory 实现来说，其实没有事务概念，直接返回 0 即可。
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
