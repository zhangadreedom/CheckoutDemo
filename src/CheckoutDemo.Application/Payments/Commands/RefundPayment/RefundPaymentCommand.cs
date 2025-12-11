using CheckoutDemo.Application.Payments.DTOs;
using MediatR;

namespace CheckoutDemo.Application.Payments.Commands.RefundPayment
{
    public sealed class RefundPaymentCommand : IRequest<PaymentDto>
    {
        public string PaymentId { get; init; }

        public RefundPaymentCommand(string paymentId)
        {
            PaymentId = paymentId;
        }
    }
}
