using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Domain.Common
{

    public interface IHasDomainEvents
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }
}
