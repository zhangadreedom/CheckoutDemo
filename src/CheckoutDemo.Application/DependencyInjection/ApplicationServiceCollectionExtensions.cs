using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutDemo.Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
