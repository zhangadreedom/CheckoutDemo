using CheckoutDemo.Application.Common.Abstractions;
using CheckoutDemo.Application.Common.Options;
using CheckoutDemo.Domain.Payments.Abstractions;
using CheckoutDemo.Infrastructure.Payments;
using CheckoutDemo.Infrastructure.Persistence.InMemory;
using CheckoutDemo.Infrastructure.Persistence.UnitOfWork;
using CheckoutDemo.Infrastructure.Time;
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
            // 绑定 CheckoutOptions
            services.Configure<CheckoutOptions>(
                configuration.GetSection(CheckoutOptions.SectionName));

            // HttpClient
            services.AddHttpClient("checkout");

            // 时间提供器
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

            // 仓储 + UoW（InMemory 版本）
            services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
            services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

            // 支付网关
            services.AddScoped<ICheckoutPaymentGateway, CheckoutPaymentGateway>();

            // IEventPublisher 暂时可以做一个 no-op 实现（如果还没写），或在 Application 层先不调用
            // services.AddScoped<IEventPublisher, NoOpEventPublisher>();

            return services;
        }
    }
}
