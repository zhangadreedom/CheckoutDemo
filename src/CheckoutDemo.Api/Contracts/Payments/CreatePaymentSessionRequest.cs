namespace CheckoutDemo.Api.Contracts.Payments
{
    public sealed class CreatePaymentSessionRequest
    {
        /// <summary>金额（minor units），例如 1999 = €19.99</summary>
        public long Amount { get; init; }

        /// <summary>货币，默认 EUR</summary>
        public string Currency { get; init; } = "EUR";

        /// <summary>账单国家，默认 NL（iDEAL 用）</summary>
        public string Country { get; init; } = "NL";

        /// <summary>业务参考号，例如订单号</summary>
        public string Reference { get; init; } = "ORDER-DEMO-001";

        /// <summary>可选：首选支付方式（Card / Ideal / ApplePay / GooglePay）</summary>
        public string? PreferredMethod { get; init; }
    }
}
