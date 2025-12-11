using CheckoutDemo.Application.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Infrastructure.Persistence.EF
{
    public sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly CheckoutDbContext _dbContext;

        public EfUnitOfWork(CheckoutDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _dbContext.SaveChangesAsync(cancellationToken);
    }
}
