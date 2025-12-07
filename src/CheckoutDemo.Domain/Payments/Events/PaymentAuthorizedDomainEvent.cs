using CheckoutDemo.Domain.Common;

namespace CheckoutDemo.Domain.Payments.Events
{

    public sealed class PaymentAuthorizedDomainEvent : IDomainEvent
    {
        public Guid PaymentId { get; }
        public string CheckoutPaymentId { get; }
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;

        public PaymentAuthorizedDomainEvent(Guid paymentId, string checkoutPaymentId)
        {
            PaymentId = paymentId;
            CheckoutPaymentId = checkoutPaymentId;
        }
    }
}
