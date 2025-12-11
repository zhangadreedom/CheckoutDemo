using CheckoutDemo.Application.Payments.Models;
using CheckoutDemo.Domain.Payments.ValueObjects;

namespace CheckoutDemo.Application.Common.Abstractions
{
    public interface ICheckoutPaymentGateway
    {
        Task<PaymentSessionResult> CreatePaymentSessionAsync(
            Money amount,
            string reference,
            string currency,
            string country,
            CancellationToken cancellationToken = default);

        // 预留：后续可以加 Capture / Refund / VerifyWebhookSignature 等方法
        Task RefundAsync(
            string checkoutPaymentId,
            Money amount,
            CancellationToken cancellationToken = default);
        // bool VerifySignature(...);
    }
}
