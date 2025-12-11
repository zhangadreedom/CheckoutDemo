using CheckoutDemo.Application.Common.Abstractions;
using CheckoutDemo.Application.Common.Exceptions;
using CheckoutDemo.Application.Payments.DTOs;
using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Domain.Payments.Entities;
using CheckoutDemo.Domain.Payments.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Payments.Commands.RefundPayment
{
    public sealed class RefundPaymentCommandHandler
        : IRequestHandler<RefundPaymentCommand, PaymentDto>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICheckoutPaymentGateway _paymentGateway;
        private readonly IUnitOfWork _unitOfWork;

        public RefundPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            ICheckoutPaymentGateway paymentGateway,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentDto> Handle(
            RefundPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByCheckoutPaymentIdAsync(request.PaymentId, cancellationToken);

            if (payment is null)
            {
                throw new NotFoundException(nameof(Payment), request.PaymentId);
            }

            if (payment.Status != PaymentStatus.Captured)
            {
                throw new BusinessRuleViolationException(
                    $"Only captured payments can be refunded. Current status: {payment.Status}");
            }

            if (string.IsNullOrWhiteSpace(payment.CheckoutPaymentId))
            {
                throw new BusinessRuleViolationException("Cannot refund a payment without CheckoutPaymentId.");
            }

          
            await _paymentGateway.RefundAsync(
                payment.CheckoutPaymentId,
                payment.Amount,
                cancellationToken);

          
            payment.MarkRefunded();
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

           
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
