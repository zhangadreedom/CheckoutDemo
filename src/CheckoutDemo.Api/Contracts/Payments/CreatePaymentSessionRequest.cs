namespace CheckoutDemo.Api.Contracts.Payments
{
    public sealed class CreatePaymentSessionRequest
    {
        public long Amount { get; init; }

        public string Currency { get; init; } = "EUR";

        public string Country { get; init; } = "NL";

        public string Reference { get; init; } = "ORDER-DEMO-001";

        public string? PreferredMethod { get; init; }
    }
}
