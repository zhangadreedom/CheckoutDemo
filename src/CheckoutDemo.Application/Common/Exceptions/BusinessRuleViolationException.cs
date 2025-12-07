using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Common.Exceptions
{
    public class BusinessRuleViolationException : Exception
    {
        public BusinessRuleViolationException(string message)
            : base(message)
        {
        }
    }
}
