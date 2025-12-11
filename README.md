# Checkout Demo

An end-to-end sample that pairs a Checkout.com payment API back end with a Vite + React front-end experience for selling iPhone cases. The solution is split into a .NET 10.0 API under `src/` and a standalone Vite workspace under `FrontEnd/`.

## Directory layout
- `FrontEnd/my-checkout-iPhonecases/` – React + TypeScript demo that renders Checkout.com Flow, lets you choose amount/country, and calls the API to create payment sessions, poll statuses, trigger webhooks, and request refunds. Vite drives local development with scripts in `package.json`.
- `src/CheckoutDemo.Api/` – Minimal API host wiring MediatR commands/queries, CORS, OpenAPI, and routes for payment sessions, payment lookups, webhooks, and refunds.
- `src/CheckoutDemo.Application/` – Application layer with MediatR handlers and behaviors registered via `AddApplication` for commands/queries.
- `src/CheckoutDemo.Domain/` – Domain entities and abstractions for payments and shared logic.
- `src/CheckoutDemo.Infrastructure/` – Infrastructure services: EF Core persistence (SQL Server connection), payment gateway integration, time provider, and webhook signature validation registration.
- `test/` – Placeholder for automated tests.

## Front-end (FrontEnd/my-checkout-iPhonecases)
The React app uses Checkout.com Web Components to render a Flow checkout experience. Users can enter amounts (minor units), pick a billing country, and toggle light/dark modes. On submission, it calls the API to create a payment session, mounts the Flow component, and simulates webhook events after payment completion. An order-details tab polls the API for status updates and allows refunds once a capture is available. Key files:
- `src/App.tsx` – UI, Flow integration, polling, and refund logic.
- `src/api.ts` – Helper functions for calling the API at `https://localhost:7084` (sessions, status, webhooks, refunds).

To run the front-end locally:
```bash
cd FrontEnd/my-checkout-iPhonecases
npm install
npm run dev
```
Vite serves the app on port 5173 by default.

## Back-end (src)
The API is a minimal ASP.NET app targeting .NET 10.0. It registers Application/Infrastructure layers, enables CORS for the Vite origin, and exposes routes for payment session creation, payment lookups, webhook intake, and refunds. MediatR mediates commands/queries and exception handling. EF Core is configured for SQL Server; update `ConnectionStrings:CheckoutDemo` in `src/CheckoutDemo.Infrastructure/appsettings.json` or user secrets for your environment.

To run the API locally:
```bash
# from the repository root
dotnet restore
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CheckoutDemo.Api
```
The API listens on HTTPS (default `https://localhost:7084`) and allows cross-origin requests from `http://localhost:5173`.
