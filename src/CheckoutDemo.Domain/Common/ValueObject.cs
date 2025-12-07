using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Domain.Common
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(0, (hash, component) =>
                {
                    unchecked
                    {
                        return (hash * 397) ^ (component?.GetHashCode() ?? 0);
                    }
                });
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
    }
}
