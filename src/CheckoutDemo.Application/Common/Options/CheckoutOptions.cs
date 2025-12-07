namespace CheckoutDemo.Application.Common.Options
{
    public class CheckoutOptions
    {
        public const string SectionName = "Checkout";

        public string PublicKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string BaseUrl { get; set; } = "https://api.sandbox.checkout.com";
        public string? ProcessingChannelId { get; set; }
    }
}
