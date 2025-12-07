using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Domain.Payments.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Authorized = 1,
        Captured = 2,
        Declined = 3,
        Refunded = 4,
        Cancelled = 5,
        Failed = 6
    }
}
