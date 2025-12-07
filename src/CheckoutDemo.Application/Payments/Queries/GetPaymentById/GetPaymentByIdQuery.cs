using CheckoutDemo.Application.Payments.DTOs;
using MediatR;

namespace CheckoutDemo.Application.Payments.Queries.GetPaymentById
{
    public sealed class GetPaymentByIdQuery : IRequest<PaymentDto?>
    {
        public Guid Id { get; init; }

        public GetPaymentByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
