using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Common.Abstractions
{
    public interface ICheckoutSignatureValidator
    {
        bool IsValid(string rawBody, IDictionary<string, string> headers);
    }
}
