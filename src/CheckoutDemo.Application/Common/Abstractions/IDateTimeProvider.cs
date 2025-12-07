using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Common.Abstractions
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
