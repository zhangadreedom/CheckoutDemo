export interface CreatePaymentSessionPayload {
  amount: number;
  currency: string;
  country: string;
  reference: string;
  preferredMethod?: string;
}

export interface CheckoutWebhookEventPayload {
  type: 'payment_approved' | 'payment_captured';
  data: {
    id: string;
    reference?: string;
  };
}

export interface PaymentStatusResponse {
  status: string;
}

export interface RefundRequest {
  amount: number;
}

export interface RefundResponse {
  id?: string;
  status?: string;
  amount?: number;
}

export async function createPaymentSession(
  payload: CreatePaymentSessionPayload
): Promise<{ publicKey: string; paymentSession: any }> {
  const requestBody = {
    preferredMethod: 'Ideal',
    ...payload,
    preferredMethod: payload.preferredMethod ?? 'Ideal'
  };

  const res = await fetch('https://localhost:7084/api/payment-sessions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(requestBody)
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Failed to create payment session: ${text}`);
  }

  return res.json();
}

export async function createCheckoutSession(params: {
  amount: number;
  country: string;
  reference: string;
  currency?: string;
  preferredMethod?: string;
}): Promise<{ publicKey: string; paymentSession: any }> {
  const { amount, country, reference, currency = 'EUR', preferredMethod } = params;
  return createPaymentSession({
    amount,
    country,
    reference,
    currency,
    preferredMethod
  });
}

export async function fetchPaymentStatus(
  referenceId: string
): Promise<PaymentStatusResponse> {
  const res = await fetch(`https://localhost:7084/api/payments/by-reference/${referenceId}`);
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Failed to fetch payment status: ${text}`);
  }
  return res.json();
}

export async function refundPayment(
  paymentId: string,
  payload: RefundRequest
): Promise<RefundResponse> {
  const res = await fetch(`https://localhost:7084/api/payments/${paymentId}/refund`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Failed to refund payment: ${text}`);
  }
  return res.json();
}

export async function sendCheckoutWebhookEvent(
  payload: CheckoutWebhookEventPayload
): Promise<void> {
  const res = await fetch('https://localhost:7084/webhooks/checkout', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Failed to send webhook event (${payload.type}): ${text}`);
  }
}
