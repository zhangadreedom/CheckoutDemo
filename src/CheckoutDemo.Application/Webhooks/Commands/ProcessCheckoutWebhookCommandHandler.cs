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
            // 1. 验签（Demo 可以先跳过真正实现，但结构先搭好）
            if (!_signatureValidator.IsValid(request.RawBody, request.Headers))
            {
                // 不合法请求，啥也不做（也可以抛异常，让 API 返回 400）
                return;
            }

            // 2. 解析 JSON
            using var doc = JsonDocument.Parse(request.RawBody);
            var root = doc.RootElement;

            var eventType = root.GetProperty("type").GetString();
            var data = root.GetProperty("data");
            var checkoutPaymentId = data.GetProperty("id").GetString();
            var reference = data.GetProperty("reference").GetString();

            if (string.IsNullOrWhiteSpace(eventType) || string.IsNullOrWhiteSpace(checkoutPaymentId) || string.IsNullOrWhiteSpace(reference))
            {
                // 无效 payload，直接返回
                return;
            }

            var payment = await _paymentRepository.GetByReferenceAsync(reference, cancellationToken);

            //// 3. 根据 CheckoutPaymentId 找本地 Payment
            //var payment = await _paymentRepository.GetByCheckoutPaymentIdAsync(
            //    checkoutPaymentId,
            //    cancellationToken);
            if (payment is null)
            {
                return;
            }

            // 4. 根据 event type 更新状态
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
                    payment.MarkFailed(); // 或者新增 Cancelled 状态，这里先用 Failed 代表终止
                    _paymentRepository.Update(payment);
                    break;

                // 你可以根据 Checkout 文档扩展更多事件类型
                default:
                    // 不关心的事件类型，直接忽略
                    break;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
