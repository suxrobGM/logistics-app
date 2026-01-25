# Customer Payments

Customers can pay invoices through multiple channels: public payment links, the customer portal, and tracking pages.

## Payment Methods

### 1. Public Payment Links

Shareable links that don't require authentication:

```text
https://portal.example.com/pay/{tenantId}/{token}
```

**Features:**

- No login required
- Secure token-based access
- Configurable expiration (default 30 days)
- Access tracking (view count, last accessed)
- Can be revoked by staff

**Use Cases:**

- Email to customers
- Embedded in tracking pages
- Shared via messaging apps

### 2. Customer Portal

Authenticated access for registered customers:

- View all invoices for their account
- Pay invoices directly
- View payment history
- Download invoices as PDF

### 3. Tracking Page Integration

When viewing a shipment tracking page:

- If invoice exists and unpaid, show "Pay Invoice" button
- Links to public payment page
- Seamless experience from tracking to payment

## Public Payment API

### Get Invoice Details

```http
GET /public/payments/{tenantId}/{token}

Response:
{
  "id": "invoice-guid",
  "number": 1001,
  "status": "Issued",
  "total": { "amount": 1500.00, "currency": "USD" },
  "amountDue": 1500.00,
  "dueDate": "2024-02-01T00:00:00Z",
  "companyName": "ABC Trucking",
  "customerName": "XYZ Logistics",
  "loadNumber": 5001,
  "lineItems": [
    {
      "description": "Freight charges",
      "type": "BaseRate",
      "amount": 1400.00,
      "quantity": 1
    },
    {
      "description": "Fuel surcharge",
      "type": "FuelSurcharge",
      "amount": 100.00,
      "quantity": 1
    }
  ],
  "canPay": true
}
```

### Create Setup Intent

For saving payment methods:

```http
POST /public/payments/{tenantId}/{token}/setup

Response:
{
  "clientSecret": "seti_..._secret_..."
}
```

### Process Payment

```http
POST /public/payments/{tenantId}/{token}/pay
{
  "paymentMethodId": "pm_1234567890",
  "amount": 500.00  // optional, for partial payments
}

Response:
{
  "paymentIntentId": "pi_1234567890",
  "clientSecret": "pi_..._secret_...",
  "status": "succeeded"  // or "requires_action"
}
```

## Partial Payments

The system supports partial payments:

1. Customer specifies amount less than total due
2. Payment recorded and applied to invoice
3. Invoice status changes to `PartiallyPaid`
4. Remaining balance shown on subsequent views
5. Additional payments until fully paid

## Payment Flow

```
1. Customer clicks payment link
2. Frontend loads invoice details (GET /public/payments/...)
3. Customer enters card details (Stripe Elements)
4. Frontend creates payment method
5. Frontend submits payment (POST /public/payments/.../pay)
6. Backend creates PaymentIntent with destination charges
7. Funds routed to trucking company's Stripe Connect account
8. Invoice status updated
9. Customer sees confirmation
```

## Manual Payments (Staff)

For cash and check payments received offline:

```http
POST /invoices/{id}/payments/manual
{
  "amount": 500.00,
  "type": "Cash",  // or "Check"
  "referenceNumber": "Check #1234",
  "notes": "Received via mail",
  "receivedDate": "2024-01-20T00:00:00Z"
}
```

**Requirements:**

- Staff must have `Invoice.Manage` permission
- Only Cash or Check payment types allowed
- Payment immediately marked as Paid
- Recorded by user ID and timestamp

## Security Considerations

### Token Security

- Tokens are cryptographically random (32 bytes)
- URL-safe encoding (no special characters)
- Unique per payment link
- Cannot be guessed or enumerated

### Access Control

- Public endpoints validate:
  - Tenant exists and has Stripe Connect enabled
  - Token is valid and not expired
  - Link hasn't been revoked
  - Invoice is in payable status

### Rate Limiting

Consider implementing rate limiting on public endpoints to prevent abuse.

## Frontend Integration

### Stripe Elements Setup

```typescript
// Initialize Stripe with connected account
const stripe = await loadStripe(publishableKey, {
  stripeAccount: connectedAccountId
});

// Create payment element
const elements = stripe.elements({
  clientSecret: setupIntent.clientSecret
});

const paymentElement = elements.create('payment');
paymentElement.mount('#payment-element');

// Handle submission
const { error, paymentIntent } = await stripe.confirmPayment({
  elements,
  confirmParams: {
    return_url: `${window.location.origin}/payment-complete`
  }
});
```

## Related Files

- Public API: `src/Presentation/Logistics.API/Controllers/PublicPaymentController.cs`
- Commands: `src/Core/Logistics.Application/Commands/Payment/`
- Queries: `src/Core/Logistics.Application/Queries/PaymentLink/`
- DTOs: `src/Shared/Logistics.Shared.Models/Invoice/PublicInvoiceDto.cs`
