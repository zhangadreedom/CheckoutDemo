using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Domain.Common
{
    public abstract class AggregateRoot : Entity, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected AggregateRoot()
        {
        }

        protected AggregateRoot(Guid id) : base(id)
        {
        }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
