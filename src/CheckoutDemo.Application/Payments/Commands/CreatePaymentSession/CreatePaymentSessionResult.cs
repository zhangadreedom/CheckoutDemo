using CheckoutDemo.Application.Payments.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Payments.Commands.CreatePaymentSession
{
    /// <summary>
    /// Command 的返回值：除了 PaymentSession，还可以顺便把 PublicKey 传给前端。
    /// </summary>
    public sealed class CreatePaymentSessionResult
    {
        public string PublicKey { get; init; } = default!;
        public PaymentSessionDto PaymentSession { get; init; } = default!;
    }
}
