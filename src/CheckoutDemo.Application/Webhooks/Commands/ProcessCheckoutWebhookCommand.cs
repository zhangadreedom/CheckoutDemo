using MediatR;

namespace CheckoutDemo.Application.Webhooks.Commands
{
    public sealed class ProcessCheckoutWebhookCommand : IRequest
    {
        public string EventType { get; init; } = default!;  // payment_approved / payment_declined 等
        public string RawBody { get; init; } = default!;    // 原始 JSON，用于调试或签名验证
        public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
    }
}
