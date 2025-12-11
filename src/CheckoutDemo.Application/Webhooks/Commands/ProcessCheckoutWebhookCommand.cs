using MediatR;

namespace CheckoutDemo.Application.Webhooks.Commands
{
    public sealed class ProcessCheckoutWebhookCommand : IRequest
    {
        public string EventType { get; init; } = default!;  
        public string RawBody { get; init; } = default!;    
        public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
