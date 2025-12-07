using MediatR;

namespace CheckoutDemo.Application.Payments.Commands.CreatePaymentSession
{
    public sealed class CreatePaymentSessionCommand : IRequest<CreatePaymentSessionResult>
    {
        /// <summary>金额（minor units）</summary>
        public long Amount { get; init; }

        /// <summary>货币（例如 "EUR"）</summary>
        public string Currency { get; init; } = "EUR";

        /// <summary>国家代码（例如 "NL"）</summary>
        public string Country { get; init; } = "NL";

        /// <summary>业务侧订单 / 支付参考号</summary>
        public string Reference { get; init; } = default!;

        /// <summary>可选：首选支付方式（Card / Ideal / ApplePay / GooglePay）</summary>
        public string? PreferredMethod { get; init; }
    }
}
