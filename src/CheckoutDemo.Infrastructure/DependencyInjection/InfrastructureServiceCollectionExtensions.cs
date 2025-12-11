using CheckoutDemo.Application.Common.Abstractions;
using CheckoutDemo.Application.Common.Options;
using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Infrastructure.Payments;
using CheckoutDemo.Infrastructure.Persistence.EF;
using CheckoutDemo.Infrastructure.Persistence.InMemory;
using CheckoutDemo.Infrastructure.Persistence.UnitOfWork;
using CheckoutDemo.Infrastructure.Time;
using CheckoutDemo.Infrastructure.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutDemo.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CheckoutOptions>(
                configuration.GetSection(CheckoutOptions.SectionName));

            // HttpClient
            services.AddHttpClient("checkout");

            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

            // InMemory
            //services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
            //services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
            var connectionString = configuration.GetConnectionString("CheckoutDemo")
                                   ?? throw new InvalidOperationException("Connection string 'CheckoutDemo' is not configured.");

            services.AddDbContext<CheckoutDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IPaymentRepository, EfPaymentRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped<ICheckoutSignatureValidator, NoOpCheckoutSignatureValidator>();

            services.AddScoped<ICheckoutPaymentGateway, CheckoutPaymentGateway>();

            return services;
        }
    }
}
