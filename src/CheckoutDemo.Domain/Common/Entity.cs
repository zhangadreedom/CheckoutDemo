using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace CheckoutDemo.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        protected Entity(Guid id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Id.Equals(other.Id);
        }

        public static bool operator ==(Entity left, Entity right) =>
            Equals(left, right);

        public static bool operator !=(Entity left, Entity right) =>
            !Equals(left, right);

        public override int GetHashCode() => Id.GetHashCode();
    }

}
