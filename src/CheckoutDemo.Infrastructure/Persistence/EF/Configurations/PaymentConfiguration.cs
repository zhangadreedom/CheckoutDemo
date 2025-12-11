using CheckoutDemo.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutDemo.Infrastructure.Persistence.EF.Configurations
{
    public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Reference)
                .IsRequired()
                .HasMaxLength(200);

            // Money 映射：Amount（long）+ CurrencyCode.Value
            builder.OwnsOne(x => x.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .IsRequired();

                money.OwnsOne(m => m.Currency, currency =>
                {
                    currency.Property(c => c.Value)
                        .HasColumnName("Currency")
                        .IsRequired()
                        .HasMaxLength(3);
                });
            });

            // BillingCountry 映射
            builder.OwnsOne(x => x.BillingCountry, country =>
            {
                country.Property(c => c.Value)
                    .HasColumnName("BillingCountry")
                    .IsRequired()
                    .HasMaxLength(2);
            });

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.MethodType)
                .HasConversion<int?>();

            builder.Property(x => x.CheckoutPaymentId)
                .HasMaxLength(200);

            builder.Property(x => x.PaymentSessionId)
                .HasMaxLength(200);

            builder.Property(x => x.PaymentSessionSecret)
                .HasMaxLength(200);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc);
        }
    }
}
