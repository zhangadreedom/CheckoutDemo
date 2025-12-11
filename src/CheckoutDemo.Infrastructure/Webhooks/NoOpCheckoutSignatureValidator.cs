using CheckoutDemo.Application.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Infrastructure.Webhooks
{
    public sealed class NoOpCheckoutSignatureValidator : ICheckoutSignatureValidator
    {
        public bool IsValid(string rawBody, IDictionary<string, string> headers)
        {
            // TODO: 使用 headers["cko-signature"] + Webhook secret 做真实校验
            return true;
        }
    }
}
