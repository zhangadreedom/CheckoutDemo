using CheckoutDemo.Application.Payments.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Application.Payments.Queries.GetPaymentByReference
{
    public sealed record GetPaymentByReferenceQuery(string Reference)
        : IRequest<PaymentDto?>;
}
