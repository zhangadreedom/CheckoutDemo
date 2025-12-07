using CheckoutDemo.Domain.Payments.Enums;

namespace CheckoutDemo.Application.Payments.DTOs
{
    public sealed class PaymentDto
    {
        public Guid Id { get; init; }
        public string Reference { get; init; } = default!;
        public long Amount { get; init; }
        public string Currency { get; init; } = default!;
        public string Country { get; init; } = default!;
        public PaymentStatus Status { get; init; }
        public string? MethodType { get; init; }
        public string? CheckoutPaymentId { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}
