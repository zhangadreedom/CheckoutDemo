using CheckoutDemo.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Domain.Payments.Events
{
    public sealed class PaymentDeclinedDomainEvent : IDomainEvent
    {
        public Guid PaymentId { get; }
        public string? CheckoutPaymentId { get; }
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;

        public PaymentDeclinedDomainEvent(Guid paymentId, string? checkoutPaymentId)
        {
            PaymentId = paymentId;
            CheckoutPaymentId = checkoutPaymentId;
        }
    }
}
