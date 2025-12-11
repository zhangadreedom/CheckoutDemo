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

        Task RefundAsync(
            string checkoutPaymentId,
            Money amount,
            CancellationToken cancellationToken = default);
        // bool VerifySignature(...);
    }
}
