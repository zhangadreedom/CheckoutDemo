using CheckoutDemo.Application.Common.Abstractions;
using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Domain.Payments.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CheckoutDemo.Application.Webhooks.Commands
{
    public sealed class ProcessCheckoutWebhookCommandHandler
        : IRequestHandler<ProcessCheckoutWebhookCommand>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckoutSignatureValidator _signatureValidator;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ProcessCheckoutWebhookCommandHandler(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            ICheckoutSignatureValidator signatureValidator)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _signatureValidator = signatureValidator;
        }

        public async Task Handle(ProcessCheckoutWebhookCommand request, CancellationToken cancellationToken)
        {

            if (!_signatureValidator.IsValid(request.RawBody, request.Headers))
            {
                return;
            }

            using var doc = JsonDocument.Parse(request.RawBody);
            var root = doc.RootElement;

            var eventType = root.GetProperty("type").GetString();
            var data = root.GetProperty("data");
            var checkoutPaymentId = data.GetProperty("id").GetString();
            var reference = data.GetProperty("reference").GetString();

            if (string.IsNullOrWhiteSpace(eventType) || string.IsNullOrWhiteSpace(checkoutPaymentId) || string.IsNullOrWhiteSpace(reference))
            {
               
                return;
            }

            var payment = await _paymentRepository.GetByReferenceAsync(reference, cancellationToken);

            if (payment is null)
            {
                return;
            }


            switch (eventType)
            {
                case "payment_approved":
                case "payment_authorized":
                    if (payment.Status is not PaymentStatus.Authorized and not PaymentStatus.Captured)
                    {
                        payment.MarkAuthorized(checkoutPaymentId);
                        _paymentRepository.Update(payment);
                    }
                    break;

                case "payment_captured":
                    if (payment.Status != PaymentStatus.Captured)
                    {
                        payment.MarkCaptured();
                        _paymentRepository.Update(payment);
                    }
                    break;

                case "payment_declined":
                    payment.MarkDeclined();
                    _paymentRepository.Update(payment);
                    break;

                case "payment_voided":
                case "payment_canceled":
                    payment.MarkFailed(); 
                    _paymentRepository.Update(payment);
                    break;

                default:
                    break;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
