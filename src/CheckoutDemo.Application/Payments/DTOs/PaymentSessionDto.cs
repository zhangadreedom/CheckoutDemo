using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Payments.DTOs
{
    public sealed class PaymentSessionDto
    {

        public string Id { get; init; } = default!;

        public string PaymentSessionSecret { get; init; } = default!;

        public object Raw { get; init; } = default!;
    }
}
