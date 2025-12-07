using CheckoutDemo.Domain.Common;
using CheckoutDemo.Domain.Payments.Enums;
using CheckoutDemo.Domain.Payments.ValueObjects;
using CheckoutDemo.Domain.Payments.Events;

namespace CheckoutDemo.Domain.Payments.Entities
{
    public sealed class Payment : AggregateRoot
    {
        // 业务相关
        public string Reference { get; private set; } = default!;          // 自己系统的 order / payment reference
        public Money Amount { get; private set; } = default!;
        public CountryCode BillingCountry { get; private set; } = default!;
        public PaymentMethodType? MethodType { get; private set; }
        public PaymentStatus Status { get; private set; }

        // 与 Checkout 对接相关的字段
        public string? CheckoutPaymentId { get; private set; }             // payment id (evt.id / data.id)
        public string? PaymentSessionId { get; private set; }              // ps_xxx
        public string? PaymentSessionSecret { get; private set; }          // pss_xxx

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        // EF / 序列化需要的无参构造函数
        private Payment()
        {
        }

        private Payment(
            string reference,
            Money amount,
            CountryCode billingCountry,
            PaymentMethodType? methodType,
            DateTime createdAtUtc)
        {
            Reference = reference;
            Amount = amount;
            BillingCountry = billingCountry;
            MethodType = methodType;
            Status = PaymentStatus.Pending;
            CreatedAtUtc = createdAtUtc;
        }

        public static Payment CreateNew(
            string reference,
            Money amount,
            CountryCode billingCountry,
            PaymentMethodType? methodType,
            DateTime createdAtUtcUtc)
        {
            var payment = new Payment(reference, amount, billingCountry, methodType, createdAtUtcUtc);

            // 可以在这里添加 “PaymentCreated” 领域事件（如果你需要）
            // payment.AddDomainEvent(new PaymentCreatedDomainEvent(payment.Id, reference));

            return payment;
        }

        public void AttachPaymentSession(string paymentSessionId, string paymentSessionSecret)
        {
            PaymentSessionId = paymentSessionId;
            PaymentSessionSecret = paymentSessionSecret;
            Touch();
        }

        public void MarkAuthorized(string checkoutPaymentId)
        {
            if (Status is PaymentStatus.Captured or PaymentStatus.Refunded)
            {
                throw new InvalidOperationException("Cannot authorize a payment that is already captured or refunded.");
            }

            Status = PaymentStatus.Authorized;
            CheckoutPaymentId = checkoutPaymentId;
            Touch();

            AddDomainEvent(new PaymentAuthorizedDomainEvent(Id, CheckoutPaymentId!));
        }

        public void MarkCaptured()
        {
            if (Status != PaymentStatus.Authorized)
            {
                throw new InvalidOperationException("Only authorized payments can be captured.");
            }

            Status = PaymentStatus.Captured;
            Touch();

            AddDomainEvent(new PaymentCapturedDomainEvent(Id, CheckoutPaymentId!));
        }

        public void MarkDeclined()
        {
            Status = PaymentStatus.Declined;
            Touch();

            AddDomainEvent(new PaymentDeclinedDomainEvent(Id, CheckoutPaymentId));
        }

        public void MarkFailed()
        {
            Status = PaymentStatus.Failed;
            Touch();

            AddDomainEvent(new PaymentFailedDomainEvent(Id, CheckoutPaymentId));
        }

        public void MarkRefunded()
        {
            if (Status != PaymentStatus.Captured)
            {
                throw new InvalidOperationException("Only captured payments can be refunded.");
            }

            Status = PaymentStatus.Refunded;
            Touch();

            AddDomainEvent(new PaymentRefundedDomainEvent(Id, CheckoutPaymentId!));
        }

        private void Touch()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
