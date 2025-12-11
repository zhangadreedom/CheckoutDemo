using CheckoutDemo.Application.Common.Abstractions;
using CheckoutDemo.Application.Common.Options;
using CheckoutDemo.Application.Payments.Models;
using CheckoutDemo.Domain.Payments.ValueObjects;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
namespace CheckoutDemo.Infrastructure.Payments
{

    public sealed class CheckoutPaymentGateway : ICheckoutPaymentGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CheckoutOptions _options;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public CheckoutPaymentGateway(
            IHttpClientFactory httpClientFactory,
            IOptions<CheckoutOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<PaymentSessionResult> CreatePaymentSessionAsync(
            Money amount,
            string reference,
            string currency,
            string country,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("checkout");

            client.BaseAddress = new Uri(_options.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.SecretKey);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                amount = amount.Amount,
                currency,
                reference,
                processing_channel_id = _options.ProcessingChannelId,
                billing = new
                {
                    address = new
                    {
                        country
                    }
                },
                success_url = "http://localhost:5173/success",
                failure_url = "http://localhost:5173/failure"
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("/payment-sessions", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Checkout payment-sessions failed: {(int)response.StatusCode} - {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            var id = root.GetProperty("id").GetString()
                     ?? throw new InvalidOperationException("Payment session id missing in response.");

            var secret = root.GetProperty("payment_session_secret").GetString()
                        ?? throw new InvalidOperationException("payment_session_secret missing in response.");

            var rawObject = JsonSerializer.Deserialize<object>(responseBody, JsonOptions)
                            ?? throw new InvalidOperationException("Failed to deserialize payment session response.");

            return new PaymentSessionResult
            {
                Id = id,
                PaymentSessionSecret = secret,
                Raw = rawObject
            };
        }

        public async Task RefundAsync(
            string checkoutPaymentId,
            Money amount,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("checkout");

            client.BaseAddress = new Uri(_options.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.SecretKey);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                amount = amount.Amount,
                reference = $"refund-{checkoutPaymentId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(
                $"/payments/{checkoutPaymentId}/refunds",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Checkout refund failed: {(int)response.StatusCode} - {responseBody}");
            }
        }
    }
}
