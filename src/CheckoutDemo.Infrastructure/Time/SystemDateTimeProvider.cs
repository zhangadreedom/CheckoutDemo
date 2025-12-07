using CheckoutDemo.Application.Common.Abstractions;

namespace CheckoutDemo.Infrastructure.Time
{
    public sealed class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
