import React, { useEffect, useRef, useState } from 'react';
import { createCheckoutSession, fetchPaymentStatus, refundPayment, sendCheckoutWebhookEvent } from './api';
import caseImage from './assets/iphone_cases1.png';
import { ulid } from 'ulid';

declare global {
  interface Window {
    CheckoutWebComponents?: any;
  }
}

type Page = 'checkout' | 'details';

const STATUS_LABELS: Record<string, string> = {
  '0': 'Pending',
  '1': 'Authorized',
  '2': 'Captured',
  '3': 'Declined',
  '4': 'Refunded',
  '5': 'Cancelled',
  '6': 'Failed',
  pending: 'Pending',
  authorized: 'Authorized',
  captured: 'Captured',
  declined: 'Declined',
  refunded: 'Refunded',
  cancelled: 'Cancelled',
  canceled: 'Cancelled',
  failed: 'Failed'
};

const getStatusLabel = (statusValue: string | number | null | undefined) => {
  if (statusValue === null || statusValue === undefined) return '-';
  const key = String(statusValue).toLowerCase();
  return STATUS_LABELS[key] ?? String(statusValue);
};

const App: React.FC = () => {
  const flowContainerRef = useRef<HTMLDivElement | null>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [activePage, setActivePage] = useState<Page>('checkout');
  const [completedPaymentId, setCompletedPaymentId] = useState<string | null>(null);
  const [orderReference, setOrderReference] = useState<string | null>(null);
  const [orderStatus, setOrderStatus] = useState<string>('pending');
  const [refundedAmount, setRefundedAmount] = useState<number | null>(null);
  const [refundLoading, setRefundLoading] = useState(false);

  const [amount, setAmount] = useState(1999); // 19.99 EUR
  const [country, setCountry] = useState('NL'); // NL -> iDEAL
  const [refundAmount, setRefundAmount] = useState<number>(amount);
  const [darkMode, setDarkMode] = useState(false);
  useEffect(() => {
    const prefersDark =
      typeof window !== 'undefined' &&
      window.matchMedia &&
      window.matchMedia('(prefers-color-scheme: dark)').matches;

    setDarkMode(prefersDark);
  }, []);

  useEffect(() => {
    if (darkMode) {
      document.body.classList.add('theme-dark');
    } else {
      document.body.classList.remove('theme-dark');
    }
  }, [darkMode]);

  const primaryButtonLabel = () => {
    if (loading) return 'Submitting...';
    return 'Submit';
  };

  const handleSubmit = async () => {
    try {
      setLoading(true);
      setMessage(null);
      setError(null);
      setActivePage('checkout');
      setCompletedPaymentId(null);
      setOrderReference(null);
      setOrderStatus('pending');
      setRefundAmount(amount);
      setRefundedAmount(null);

      if (typeof window.CheckoutWebComponents !== 'function') {
        setError('CheckoutWebComponents script not loaded.');
        return;
      }

      const reference = `ORDER-${ulid()}`;
      const { paymentSession } = await createCheckoutSession({
        amount,
        country,
        reference,
        currency: 'EUR'
      });

      const checkout = await window.CheckoutWebComponents({
        paymentSession,
        publicKey: import.meta.env.VITE_CHECKOUT_PUBLIC_KEY,
        environment: 'sandbox',
        onReady: () => {
          console.log('Flow is ready');
        },
        onPaymentCompleted: async (_component: any, paymentResponse: any) => {
          console.log('Payment completed:', paymentResponse);
          setError(null);
          setMessage(`Payment succeeded (id: ${paymentResponse.id})`);
          setCompletedPaymentId(paymentResponse.id);
          const referenceToSend = paymentResponse.reference ?? reference;
          setOrderReference(referenceToSend);
          setActivePage('details');

          try {
            await sendCheckoutWebhookEvent({
              type: 'payment_approved',
              data: {
                id: paymentResponse.id,
                reference: referenceToSend
              }
            });

            await sendCheckoutWebhookEvent({
              type: 'payment_captured',
              data: {
                id: paymentResponse.id,
                reference: referenceToSend
              }
            });
          } catch (webhookError: any) {
            console.error('Failed to send checkout webhook events', webhookError);
            setError('Payment succeeded but webhook dispatch failed.');
          }
        },
        onError: (err: any) => {
          console.error('Flow error', err);
          setError('Payment failed or canceled.');
        }
      });

      if (flowContainerRef.current) {
        flowContainerRef.current.innerHTML = '';
      }
      const flowComponent = checkout.create('flow');
      if (flowContainerRef.current) {
        flowComponent.mount(flowContainerRef.current);
      }
    } catch (e: any) {
      console.error(e);
      setError(e.message ?? 'Unknown error');
    } finally {
      setLoading(false);
    }
  };

  const formattedAmount = (amount / 100).toFixed(2);
  const isDetailsPage = activePage === 'details';
  const isCaptured = getStatusLabel(orderStatus) === 'Captured';

  const handleRefund = async () => {
    if (!completedPaymentId) {
      setError('No payment ID to refund.');
      return;
    }
    if (!isCaptured) {
      setError('Refund is only available when the payment is captured.');
      return;
    }
    if (refundAmount <= 0 || refundAmount > amount) {
      setError(`Refund amount must be between 1 and ${amount} (minor units).`);
      return;
    }

    try {
      setRefundLoading(true);
      setError(null);
      const response = await refundPayment(completedPaymentId, { amount: refundAmount });
      const statusText = response.status ?? 'Refunded';
      setOrderStatus(statusText);
      setRefundedAmount(refundAmount);
      setMessage(
        `Refund requested for EUR ${(refundAmount / 100).toFixed(2)}${
          response.id ? ` (id: ${response.id})` : ''
        }`
      );
    } catch (refundErr: any) {
      console.error('Refund failed', refundErr);
      setError(refundErr?.message ?? 'Refund failed.');
    } finally {
      setRefundLoading(false);
    }
  };

  useEffect(() => {
    if (!orderReference || !isDetailsPage) return;

    let attempts = 0;
    let cancelled = false;
    let timer: ReturnType<typeof setTimeout> | null = null;

    const poll = async () => {
      attempts += 1;
      try {
        const data = await fetchPaymentStatus(orderReference);
        setOrderStatus(data.status);
      } catch (statusErr) {
        console.error('Failed to refresh status', statusErr);
      }

      if (attempts < 10 && !cancelled) {
        timer = setTimeout(poll, 2500);
      }
    };

    poll();

    return () => {
      cancelled = true;
      if (timer) clearTimeout(timer);
    };
  }, [orderReference, isDetailsPage]);

  const renderTabs = () => (
    <div className="tab-bar">
      <button
        type="button"
        className={`tab-button ${activePage === 'checkout' ? 'tab-button--active' : ''}`}
        onClick={() => setActivePage('checkout')}
      >
        Checkout
      </button>
      <button
        type="button"
        className={`tab-button ${activePage === 'details' ? 'tab-button--active' : ''}`}
        onClick={() => setActivePage('details')}
        disabled={!completedPaymentId}
        title={completedPaymentId ? '' : 'Complete a payment to view details'}
      >
        Order details
      </button>
    </div>
  );

  const renderCheckoutPage = () => (
    <>
      <div className="app-left">
        <div className="brand-badge">
          <div className="brand-circle" />
          <span>iPhone Case Studio</span>
        </div>

        <h1 className="app-title">Secure checkout</h1>
        <p className="app-subtitle">
          Selling iPhone cases in Hong Kong and the Netherlands with card, iDEAL and wallets.
        </p>

        <div className="order-card">
          <div className="order-header-row">
            <span className="order-title">Your basket</span>
            <span className="order-badge">Test mode</span>
          </div>

          <div className="order-item">
            <img
              className="order-item-thumb"
              src={caseImage}
              alt="Matte olive iPhone case"
            />
            <div className="order-item-main">
              <div className="order-item-name">Matte olive iPhone Case</div>
              <div className="order-item-meta">iPhone 15 Pro - Midnight green</div>
            </div>
            <div className="order-item-price">EUR {formattedAmount}</div>
          </div>

          <div className="order-summary-row">
            <span className="order-summary-label">Subtotal</span>
            <span className="order-summary-value">EUR {formattedAmount}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Shipping</span>
            <span className="order-summary-value">Free (test)</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Total</span>
            <span className="order-summary-value">EUR {formattedAmount}</span>
          </div>
        </div>

        <div className="form-group">
          <label className="label">Amount (EUR)</label>
          <input
            className="input"
            type="number"
            value={amount}
            min={1}
            onChange={e => setAmount(Number(e.target.value))}
          />
          <p className="helper-text">
            Using minor units. 1999 = 19.99. For the demo you can adjust this to see how the session
            reacts.
          </p>
        </div>

        <div className="form-group">
          <label className="label">Billing country</label>
          <select
            className="select"
            value={country}
            onChange={e => setCountry(e.target.value)}
          >
            <option value="NL">Netherlands (iDEAL available)</option>
            <option value="HK">Hong Kong</option>
            <option value="GB">United Kingdom</option>
            <option value="DE">Germany</option>
          </select>
          <p className="helper-text">
            For iDEAL, use NL + EUR. Wallet availability depends on your device and test account
            configuration.
          </p>
        </div>

        <button
          type="button"
          className="primary-button"
          onClick={handleSubmit}
          disabled={loading}
        >
          {primaryButtonLabel()}
        </button>

        <div className="chip-row">
          <span className="chip">PCI compliant via Flow</span>
          <span className="chip">Multi-method checkout</span>
          <span className="chip">Sandbox keys only</span>
        </div>
      </div>

      <div className="app-right">
        <div className="section-title-row">
          <div>
            <div className="section-title">Payment methods</div>
            <div className="section-subtitle">
              Card, iDEAL and Apple / Google Pay via Checkout.com Flow.
            </div>
          </div>
          <button
            type="button"
            className="theme-toggle"
            onClick={() => setDarkMode(v => !v)}
          >
            <span>{darkMode ? 'Light' : 'Dark'}</span>
            <span>{darkMode ? 'Moon' : 'Sun'}</span>
          </button>
        </div>

        {loading && (
          <div className="status-text">
            Creating payment session... Please wait a moment.
            <div className="skeleton" />
          </div>
        )}

        {message && (
          <div className="status-text status-text--success">
            Success: {message}
          </div>
        )}

        {error && (
          <div className="status-text status-text--error">
            Error: {error}
          </div>
        )}

        <div className="flow-card">
          <div
            ref={flowContainerRef}
            id="flow-container"
          />
        </div>
      </div>
    </>
  );

  const renderOrderDetailsPage = () => (
    <>
      <div className="app-left">
        <div className="brand-badge">
          <div className="brand-circle" />
          <span>iPhone Case Studio</span>
        </div>
        <h1 className="app-title">Order details</h1>
        <p className="app-subtitle">
          Thanks for your purchase. We are confirming your payment and finalizing your order.
        </p>

        <div className="order-card">
          <div className="order-header-row">
            <span className="order-title">Order summary</span>
            <span className="order-badge">In review</span>
          </div>

          <div className="order-item">
            <img
              className="order-item-thumb"
              src={caseImage}
              alt="Matte olive iPhone case"
            />
            <div className="order-item-main">
              <div className="order-item-name">Matte olive iPhone Case</div>
              <div className="order-item-meta">Payment ref: {orderReference ?? 'N/A'}</div>
              <div className="order-item-meta">Payment method: Flow (sandbox)</div>
            </div>
            <div className="order-item-price">EUR {formattedAmount}</div>
          </div>

          <div className="order-summary-row">
            <span className="order-summary-label">Order reference</span>
            <span className="order-summary-value">{orderReference ?? '-'}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Payment ID</span>
            <span className="order-summary-value">{completedPaymentId ?? '-'}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Billing country</span>
            <span className="order-summary-value">{country}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Amount</span>
            <span className="order-summary-value">EUR {formattedAmount}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Status</span>
            <span className="order-summary-value">{getStatusLabel(orderStatus)}</span>
          </div>
        </div>

        <div className="chip-row">
          <span className="chip">Auto-refreshing status</span>
          <span className="chip">Sandbox</span>
        </div>
      </div>

      <div className="app-right">
        <div className="section-title-row">
          <div>
            <div className="section-title">Order status</div>
          </div>
          <button
            type="button"
            className="theme-toggle"
            onClick={() => setDarkMode(v => !v)}
          >
            <span>{darkMode ? 'Light' : 'Dark'}</span>
            <span>{darkMode ? 'Moon' : 'Sun'}</span>
          </button>
        </div>

        {message && (
          <div className="status-text status-text--success">
            Success: {message}
          </div>
        )}

        {error && (
          <div className="status-text status-text--error">
            Error: {error}
          </div>
        )}

        <div className="flow-card">
          <div className="order-summary-row">
            <span className="order-summary-label">Payment ID</span>
            <span className="order-summary-value">{completedPaymentId ?? '-'}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Reference</span>
            <span className="order-summary-value">{orderReference ?? '-'}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Status</span>
            <span className="order-summary-value">{getStatusLabel(orderStatus)}</span>
          </div>
          <div className="order-summary-row">
            <span className="order-summary-label">Amount</span>
            <span className="order-summary-value">EUR {formattedAmount}</span>
          </div>
          {refundedAmount !== null && (
            <div className="order-summary-row">
              <span className="order-summary-label">Refund</span>
              <span className="order-summary-value">EUR {(refundedAmount / 100).toFixed(2)}</span>
            </div>
          )}
          <p className="helper-text" style={{ marginTop: '12px' }}>
            We will refresh the payment status automatically for you. If you leave this page early,
            please check your email for updates.
          </p>

          <div className="form-group" style={{ marginTop: '16px' }}>
            <label className="label">Refund amount (minor units)</label>
            <input
              className="input"
              type="number"
              min={1}
              max={amount}
              value={refundAmount}
              onChange={e => setRefundAmount(Number(e.target.value))}
            />
            <p className="helper-text">
              Enter an amount up to EUR {(amount / 100).toFixed(2)}. Refunds are allowed when status
              is Captured.
            </p>
            {!refundedAmount && (
              <>
                <button
                  type="button"
                  className="primary-button"
                  onClick={handleRefund}
                  disabled={refundLoading || !isCaptured}
                >
                  {refundLoading ? 'Processing refund...' : 'Refund'}
                </button>
                {!isCaptured && (
                  <p className="helper-text">Refund becomes available after capture.</p>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </>
  );

  return (
    <div className="app-root">
      <div className="app-shell">
        {renderTabs()}
        <div className="app-body">
          {activePage === 'checkout' ? renderCheckoutPage() : renderOrderDetailsPage()}
        </div>
      </div>
    </div>
  );
};

export default App;
