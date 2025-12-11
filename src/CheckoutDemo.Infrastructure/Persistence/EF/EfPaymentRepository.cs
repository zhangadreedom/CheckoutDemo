using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Infrastructure.Persistence.EF
{
    public sealed class EfPaymentRepository : IPaymentRepository
    {
        private readonly CheckoutDbContext _dbContext;
        private readonly ILogger<EfPaymentRepository> _logger;

        public EfPaymentRepository(CheckoutDbContext dbContext, ILogger<EfPaymentRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            return _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Reference == reference, cancellationToken);
        }

        public Task<Payment?> GetByCheckoutPaymentIdAsync(
            string checkoutPaymentId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Payments
                .FirstOrDefaultAsync(p => p.CheckoutPaymentId == checkoutPaymentId, cancellationToken);
        }

        public Task<Payment?> GetByPaymentSessionIdAsync(
            string paymentSessionId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Payments
                .FirstOrDefaultAsync(p => p.PaymentSessionId == paymentSessionId, cancellationToken);
        }

        public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EF AddAsync called. PaymentId={PaymentId}, Reference={Reference}",
    payment.Id, payment.Reference);
            await _dbContext.Payments.AddAsync(payment, cancellationToken);
        }

        public void Update(Payment payment)
        {
            _dbContext.Payments.Update(payment);
        }
    }
}
