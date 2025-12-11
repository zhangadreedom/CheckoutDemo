using CheckoutDemo.Domain.Payments.Entities;

namespace CheckoutDemo.Domain.Payments.Abstractions
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

        Task<Payment?> GetByCheckoutPaymentIdAsync(
            string checkoutPaymentId,
            CancellationToken cancellationToken = default);

        Task<Payment?> GetByPaymentSessionIdAsync(
            string paymentSessionId,
            CancellationToken cancellationToken = default);

        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

        void Update(Payment payment);
    }
}
