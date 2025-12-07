using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Payments.Models
{
    public sealed class PaymentSessionResult
    {
        public string Id { get; init; } = default!;
        public string PaymentSessionSecret { get; init; } = default!;
        public object Raw { get; init; } = default!;
    }
}
