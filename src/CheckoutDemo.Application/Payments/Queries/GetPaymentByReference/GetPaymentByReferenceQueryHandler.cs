using CheckoutDemo.Application.Payments.DTOs;
using CheckoutDemo.Domain.Payments.Abstractions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Payments.Queries.GetPaymentByReference
{
    public sealed class GetPaymentByReferenceQueryHandler
        : IRequestHandler<GetPaymentByReferenceQuery, PaymentDto?>
    {
        private readonly IPaymentRepository _paymentReadRepository;

        public GetPaymentByReferenceQueryHandler(IPaymentRepository paymentReadRepository)
        {
            _paymentReadRepository = paymentReadRepository;
        }

        public async Task<PaymentDto?> Handle(
            GetPaymentByReferenceQuery request,
            CancellationToken cancellationToken)
        {
            var payment = await _paymentReadRepository
                .GetByReferenceAsync(request.Reference, cancellationToken);

            if (payment is null)
                return null;

            return new PaymentDto
            {
                Id = payment.Id,
                Reference = payment.Reference,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency.Value,
                Country = payment.BillingCountry.Value,
                Status = payment.Status,
                MethodType = payment.MethodType?.ToString(),
                CheckoutPaymentId = payment.CheckoutPaymentId,
                CreatedAtUtc = payment.CreatedAtUtc
            };
        }
    }
}
