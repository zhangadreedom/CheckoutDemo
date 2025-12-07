using System;
using System.Collections.Generic;
using System.Text;
using CheckoutDemo.Domain.Common;

namespace CheckoutDemo.Domain.Payments.ValueObjects
{
    public sealed class CountryCode : ValueObject
    {
        public string Value { get; }

        private CountryCode(string value)
        {
            Value = value.ToUpperInvariant();
        }

        public static CountryCode From(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Country code cannot be null or empty.", nameof(code));
            }

            if (code.Length != 2)
            {
                throw new ArgumentException("Country code must be 2 characters (ISO 3166-1 alpha-2).", nameof(code));
            }

            return new CountryCode(code);
        }

        public override string ToString() => Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
