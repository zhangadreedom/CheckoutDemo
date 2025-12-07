using CheckoutDemo.Application.Common.Abstractions;
using CheckoutDemo.Application.Common.Exceptions;
using CheckoutDemo.Application.Payments.DTOs;
using CheckoutDemo.Domain.Payments.Entities;
using CheckoutDemo.Domain.Payments.Enums;
using CheckoutDemo.Domain.Payments.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;
using CheckoutDemo.Application.Common.Options;

namespace CheckoutDemo.Application.Payments.Commands.CreatePaymentSession
{
    public sealed class CreatePaymentSessionCommandHandler
        : IRequestHandler<CreatePaymentSessionCommand, CreatePaymentSessionResult>
    {
        private readonly ICheckoutPaymentGateway _paymentGateway;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CheckoutOptions _options;

        // 你可以在构造函数里注入 publicKey（比如 IConfiguration / IOptions）
        public CreatePaymentSessionCommandHandler(
            ICheckoutPaymentGateway paymentGateway,
            IDateTimeProvider dateTimeProvider,
            IUnitOfWork unitOfWork,
            IOptions<CheckoutOptions> options)
        {
            _paymentGateway = paymentGateway;
            _dateTimeProvider = dateTimeProvider;
            _unitOfWork = unitOfWork;
            _options = options.Value;
        }

        public async Task<CreatePaymentSessionResult> Handle(
            CreatePaymentSessionCommand request,
            CancellationToken cancellationToken)
        {
            // 1. 基本校验（可以用 FluentValidation 做更复杂校验）
            if (request.Amount <= 0)
            {
                throw new BusinessRuleViolationException("Amount must be greater than zero.");
            }

            // 2. 构造领域对象
            var money = Money.FromMinorUnits(request.Amount, request.Currency);
            var country = CountryCode.From(request.Country);
            PaymentMethodType? methodType = request.PreferredMethod switch
            {
                "Card" => PaymentMethodType.Card,
                "Ideal" => PaymentMethodType.Ideal,
                "ApplePay" => PaymentMethodType.ApplePay,
                "GooglePay" => PaymentMethodType.GooglePay,
                null => null,
                _ => null
            };

            var payment = Payment.CreateNew(
                reference: request.Reference,
                amount: money,
                billingCountry: country,
                methodType: methodType,
                createdAtUtcUtc: _dateTimeProvider.UtcNow);

            // TODO: 保存 Payment 到仓储（IPaymentRepository），这里先略过仓储接口，后续基础设施再补

            // 3. 调用网关创建 Checkout Payment Session
            var sessionResult = await _paymentGateway.CreatePaymentSessionAsync(
                amount: money,
                reference: request.Reference,
                currency: request.Currency,
                country: request.Country,
                cancellationToken: cancellationToken);

            // 4. 把 Session 信息挂到 Payment 上（保持本地与 Checkout 对齐）
            payment.AttachPaymentSession(
                sessionResult.Id,
                sessionResult.PaymentSessionSecret);

            // TODO: 持久化 + 发布领域事件
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. 映射成返回 DTO
            var dto = new PaymentSessionDto
            {
                Id = sessionResult.Id,
                PaymentSessionSecret = sessionResult.PaymentSessionSecret,
                Raw = sessionResult.Raw
            };

            return new CreatePaymentSessionResult
            {
                PublicKey = _options.PublicKey,
                PaymentSession = dto
            };
        }
    }
}
