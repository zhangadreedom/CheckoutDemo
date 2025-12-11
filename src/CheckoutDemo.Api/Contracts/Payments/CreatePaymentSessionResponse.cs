namespace CheckoutDemo.Api.Contracts.Payments
{
    public sealed class CreatePaymentSessionResponse
    {
        /// <summary>Checkout 公钥，用于前端 Flow 初始化</summary>
        public string PublicKey { get; init; } = default!;

        /// <summary>原始 Payment Session JSON，前端 Flow 直接使用</summary>
        public object PaymentSession { get; init; } = default!;
    }
}
