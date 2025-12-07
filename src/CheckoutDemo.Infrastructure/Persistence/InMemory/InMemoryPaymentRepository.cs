using System.Collections.Concurrent;
using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Domain.Payments.Entities;

namespace CheckoutDemo.Infrastructure.Persistence.InMemory
{
    public sealed class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly ConcurrentDictionary<Guid, Payment> _byId = new();
        private readonly ConcurrentDictionary<string, Guid> _byReference = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, Guid> _byCheckoutPaymentId = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, Guid> _byPaymentSessionId = new(StringComparer.OrdinalIgnoreCase);

        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _byId.TryGetValue(id, out var payment);
            return Task.FromResult<Payment?>(payment);
        }

        public Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (_byReference.TryGetValue(reference, out var id))
            {
                return GetByIdAsync(id, cancellationToken);
            }

            return Task.FromResult<Payment?>(null);
        }

        public Task<Payment?> GetByCheckoutPaymentIdAsync(
            string checkoutPaymentId,
            CancellationToken cancellationToken = default)
        {
            if (_byCheckoutPaymentId.TryGetValue(checkoutPaymentId, out var id))
            {
                return GetByIdAsync(id, cancellationToken);
            }

            return Task.FromResult<Payment?>(null);
        }

        public Task<Payment?> GetByPaymentSessionIdAsync(
            string paymentSessionId,
            CancellationToken cancellationToken = default)
        {
            if (_byPaymentSessionId.TryGetValue(paymentSessionId, out var id))
            {
                return GetByIdAsync(id, cancellationToken);
            }

            return Task.FromResult<Payment?>(null);
        }

        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (!_byId.TryAdd(payment.Id, payment))
            {
                throw new InvalidOperationException($"Payment with id {payment.Id} already exists.");
            }

            if (!string.IsNullOrWhiteSpace(payment.Reference))
            {
                _byReference[payment.Reference] = payment.Id;
            }

            if (!string.IsNullOrWhiteSpace(payment.CheckoutPaymentId))
            {
                _byCheckoutPaymentId[payment.CheckoutPaymentId] = payment.Id;
            }

            if (!string.IsNullOrWhiteSpace(payment.PaymentSessionId))
            {
                _byPaymentSessionId[payment.PaymentSessionId] = payment.Id;
            }

            return Task.CompletedTask;
        }

        public void Update(Payment payment)
        {
            _byId[payment.Id] = payment;

            if (!string.IsNullOrWhiteSpace(payment.Reference))
            {
                _byReference[payment.Reference] = payment.Id;
            }

            if (!string.IsNullOrWhiteSpace(payment.CheckoutPaymentId))
            {
                _byCheckoutPaymentId[payment.CheckoutPaymentId] = payment.Id;
            }

            if (!string.IsNullOrWhiteSpace(payment.PaymentSessionId))
            {
                _byPaymentSessionId[payment.PaymentSessionId] = payment.Id;
            }
        }
    }
}
