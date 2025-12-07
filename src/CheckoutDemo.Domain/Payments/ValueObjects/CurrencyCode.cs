using System;
using System.Collections.Generic;
using System.Text;
using CheckoutDemo.Domain.Common;

namespace CheckoutDemo.Domain.Payments.ValueObjects
{
    public sealed class CurrencyCode : ValueObject
    {
        public string Value { get; }

        private CurrencyCode(string value)
        {
            Value = value.ToUpperInvariant();
        }

        public static CurrencyCode From(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Currency code cannot be null or empty.", nameof(code));
            }

            if (code.Length != 3)
            {
                throw new ArgumentException("Currency code must be 3 characters (ISO 4217).", nameof(code));
            }

            return new CurrencyCode(code);
        }

        public override string ToString() => Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
