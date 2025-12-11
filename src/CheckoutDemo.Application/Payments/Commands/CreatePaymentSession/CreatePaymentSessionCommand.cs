using CheckoutDemo.Application.Payments.Models;
using MediatR;

namespace CheckoutDemo.Application.Payments.Commands.CreatePaymentSession
{
    public sealed class CreatePaymentSessionCommand : IRequest<CreatePaymentSessionResult>
    {
        public long Amount { get; init; }

        public string Currency { get; init; } = "EUR";

        public string Country { get; init; } = "NL";

        public string Reference { get; init; } = default!;

        public string? PreferredMethod { get; init; }
    }
}
