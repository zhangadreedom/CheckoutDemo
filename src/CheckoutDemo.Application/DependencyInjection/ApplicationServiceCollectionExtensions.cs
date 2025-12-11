using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutDemo.Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // 注册 MediatR：扫描当前程序集中的所有 IRequestHandler / INotificationHandler
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // 如果后面要加 Pipeline Behavior（例如日志、验证），也在这里注册：
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
