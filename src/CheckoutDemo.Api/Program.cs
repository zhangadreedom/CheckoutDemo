using CheckoutDemo.Api.Contracts.Payments;
using CheckoutDemo.Application.Common.Exceptions;
using CheckoutDemo.Application.DependencyInjection;
using CheckoutDemo.Application.Payments.Commands.CreatePaymentSession;
using CheckoutDemo.Application.Payments.Commands.RefundPayment;
using CheckoutDemo.Application.Payments.Queries.GetPaymentById;
using CheckoutDemo.Application.Payments.Queries.GetPaymentByReference;
using CheckoutDemo.Application.Webhooks.Commands;
using CheckoutDemo.Infrastructure.DependencyInjection;
using MediatR;

namespace CheckoutDemo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // 注册 Application & Infrastructure
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Logging.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);

            var app = builder.Build();

            app.UseCors();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/", () => "Checkout Demo API is running.");

            app.MapPost("/api/payment-sessions",
                async (CreatePaymentSessionRequest request, IMediator mediator, CancellationToken ct) =>
                {
                    var command = new CreatePaymentSessionCommand
                    {
                        Amount = request.Amount,
                        Currency = request.Currency,
                        Country = request.Country,
                        Reference = request.Reference,
                        PreferredMethod = request.PreferredMethod
                    };

                    var result = await mediator.Send(command, ct);

                    return Results.Ok(new
                    {
                        paymentSession = result.PaymentSession
                    });
                });
            app.MapGet("/api/payments/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    try
                    {
                        var query = new GetPaymentByIdQuery(id);
                        var result = await mediator.Send(query, ct);

                        return Results.Ok(result);
                    }
                    catch (NotFoundException)
                    {
                        return Results.NotFound();
                    }
                });
            app.MapGet("/api/payments/by-reference/{reference}",
                async (string reference, IMediator mediator, CancellationToken ct) =>
                {
                    var query = new GetPaymentByReferenceQuery(reference);
                    var result = await mediator.Send(query, ct);

                    return result is null
                        ? Results.NotFound()
                        : Results.Ok(result);
                });
            app.MapPost("/webhooks/checkout",
                async (HttpRequest httpRequest, IMediator mediator, CancellationToken ct) =>
                {
                    httpRequest.EnableBuffering();
                    using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    httpRequest.Body.Position = 0;

                    var headers = httpRequest.Headers
                        .ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase);

                    var command = new ProcessCheckoutWebhookCommand
                    {
                        RawBody = body,
                        Headers = headers
                    };

                    await mediator.Send(command, ct);

                    return Results.Ok();
                });

            app.MapPost("/api/payments/{paymentId}/refund",
                async (string paymentId, IMediator mediator, CancellationToken ct) =>
                {
                    var command = new RefundPaymentCommand(paymentId);

                    try
                    {
                        var result = await mediator.Send(command, ct);
                        return Results.Ok(result);
                    }
                    catch (NotFoundException)
                    {
                        return Results.NotFound();
                    }
                    catch (BusinessRuleViolationException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                });

            app.Run();
        }
    }
}
