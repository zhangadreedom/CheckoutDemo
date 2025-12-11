namespace CheckoutDemo.Api.Contracts.Payments
{
    public sealed class CreatePaymentSessionResponse
    {
        public string PublicKey { get; init; } = default!;

        public object PaymentSession { get; init; } = default!;
    }
}
