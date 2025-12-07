using System;
using System.Collections.Generic;
using System.Text;
using CheckoutDemo.Domain.Common;

namespace CheckoutDemo.Domain.Payments.ValueObjects
{
    public sealed class Money : ValueObject
    {
        public long Amount { get; }
        public CurrencyCode Currency { get; }

        private Money(long amount, CurrencyCode currency)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            }

            Amount = amount;
            Currency = currency;
        }

        public static Money FromMinorUnits(long amount, string currencyCode)
            => new(amount, CurrencyCode.From(currencyCode));

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
            {
                throw new InvalidOperationException("Cannot add amounts with different currencies.");
            }

            return new Money(checked(Amount + other.Amount), Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
            {
                throw new InvalidOperationException("Cannot subtract amounts with different currencies.");
            }

            if (other.Amount > Amount)
            {
                throw new InvalidOperationException("Resulting amount cannot be negative.");
            }

            return new Money(Amount - other.Amount, Currency);
        }

        public override string ToString() => $"{Amount} {Currency}";

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
