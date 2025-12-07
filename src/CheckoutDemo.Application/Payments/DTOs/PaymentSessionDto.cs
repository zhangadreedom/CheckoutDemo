using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Payments.DTOs
{
    public sealed class PaymentSessionDto
    {
        /// <summary>Checkout payment session id (ps_...)</summary>
        public string Id { get; init; } = default!;
        /// <summary>Checkout payment_session_secret (pss_...)</summary>
        public string PaymentSessionSecret { get; init; } = default!;
        /// <summary>原始返回 payload（前端 Flow 可以直接用）</summary>
        public object Raw { get; init; } = default!;
    }
}
