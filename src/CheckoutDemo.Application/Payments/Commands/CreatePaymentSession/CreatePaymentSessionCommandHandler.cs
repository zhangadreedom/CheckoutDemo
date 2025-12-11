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

        public async Task<CreatePaymentSessionResult> Handle(
            CreatePaymentSessionCommand request,
            CancellationToken cancellationToken)
        {
          
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

            var payment = Payment.CreateNew(
                reference: request.Reference,
                amount: amount,
                billingCountry: country,
                methodType: methodType,
                createdAtUtc: _dateTimeProvider.UtcNow);

            
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            
            var sessionResult = await _paymentGateway.CreatePaymentSessionAsync(
                amount,
                request.Reference,
                request.Currency,
                request.Country,
                cancellationToken);

            
            payment.AttachPaymentSession(
                sessionResult.Id,
                sessionResult.PaymentSessionSecret);

            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CheckoutDemo.Application.Payments.Models.CreatePaymentSessionResult(sessionResult.Raw);
        }
    }
}
