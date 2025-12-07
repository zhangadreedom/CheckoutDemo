using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Common.Abstractions
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
