using CheckoutDemo.Application.Common.Abstractions;
using CheckoutDemo.Application.Common.Options;
using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Domain.Payments.Entities;
using CheckoutDemo.Domain.Payments.Enums;
using CheckoutDemo.Domain.Payments.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;
using CheckoutDemo.Application.Payments.Models;

namespace CheckoutDemo.Application.Payments.Commands.CreatePaymentSession
{
    public sealed class CreatePaymentSessionCommandHandler
        : IRequestHandler<CreatePaymentSessionCommand, CreatePaymentSessionResult>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICheckoutPaymentGateway _paymentGateway;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CheckoutOptions _options;

        public CreatePaymentSessionCommandHandler(
            IPaymentRepository paymentRepository,
            ICheckoutPaymentGateway paymentGateway,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            IOptions<CheckoutOptions> options)
                {
                    _paymentRepository = paymentRepository;
                    _paymentGateway = paymentGateway;
                    _unitOfWork = unitOfWork;
                    _dateTimeProvider = dateTimeProvider;
                    _options = options.Value;
                }
        // 你可以在构造函数里注入 publicKey（比如 IConfiguration / IOptions）
        //public CreatePaymentSessionCommandHandler(
        //    ICheckoutPaymentGateway paymentGateway,
        //    IDateTimeProvider dateTimeProvider,
        //    IUnitOfWork unitOfWork,
        //    IOptions<CheckoutOptions> options)
        //{
        //    _paymentGateway = paymentGateway;
        //    _dateTimeProvider = dateTimeProvider;
        //    _unitOfWork = unitOfWork;
        //    _options = options.Value;
        //}

        public async Task<CreatePaymentSessionResult> Handle(
            CreatePaymentSessionCommand request,
            CancellationToken cancellationToken)
        {
            // 1. 构造领域值对象
            var amount = Money.FromMinorUnits(request.Amount, request.Currency);
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
            // 2. 创建 Payment 聚合根（按你自己的工厂方法来）
            var payment = Payment.CreateNew(
                reference: request.Reference,
                amount: amount,
                billingCountry: country,
                methodType: methodType,
                createdAtUtc: _dateTimeProvider.UtcNow);

            // 3. 先保存一条 Pending 记录到数据库（可选）
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. 调用 Checkout 创建 Payment Session
            var sessionResult = await _paymentGateway.CreatePaymentSessionAsync(
                amount,
                request.Reference,
                request.Currency,
                request.Country,
                cancellationToken);

            // 5. 回写 Session 信息到 Payment
            payment.AttachPaymentSession(
                sessionResult.Id,
                sessionResult.PaymentSessionSecret);

            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CheckoutDemo.Application.Payments.Models.CreatePaymentSessionResult(sessionResult.Raw);
            // 5. 映射成返回 DTO
            //var dto = new PaymentSessionDto
            //{
            //    Id = sessionResult.Id,
            //    PaymentSessionSecret = sessionResult.PaymentSessionSecret,
            //    Raw = sessionResult.Raw
            //};
            //// 6. 返回给前端
            //return new CreatePaymentSessionResult
            //{
            //    PublicKey = _options.PublicKey,
            //    PaymentSession = dto
            //};
        }
    }
}
