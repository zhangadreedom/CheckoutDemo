using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Domain.Common
{

    public interface IDomainEvent
    {
        DateTime OccurredOnUtc { get; }
    }
}
