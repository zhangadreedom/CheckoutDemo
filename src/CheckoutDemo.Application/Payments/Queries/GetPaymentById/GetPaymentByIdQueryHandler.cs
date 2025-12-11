using CheckoutDemo.Application.Common.Exceptions;
using CheckoutDemo.Application.Payments.DTOs;
using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Domain.Payments.Entities;
using MediatR;

namespace CheckoutDemo.Application.Payments.Queries.GetPaymentById
{
    public sealed class GetPaymentByIdQueryHandler
        : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);

            if (payment is null)
            {
                throw new NotFoundException(nameof(Payment), request.Id);
            }

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
