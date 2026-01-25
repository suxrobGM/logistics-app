# Stripe Connect Integration

The platform uses Stripe Connect to route payments directly to trucking company bank accounts.

## Overview

- **Account Type**: Express (simplified onboarding)
- **Charge Type**: Destination charges (funds go to connected account)
- **Platform Fee**: Configurable (default 0%), extensible for future subscription tiers

## Architecture

```text
Customer Payment → Platform Stripe Account → Connected Account (Trucking Company)
                         ↓
                   Optional Platform Fee
```

### Service Separation

Stripe Connect is implemented as a separate service from the existing subscription Stripe service:

- `IStripeService` - Platform subscriptions, payment methods
- `IStripeConnectService` - Connect account management, destination charges

## Onboarding Flow

### 1. Create Connected Account

```http
POST /stripe/connect/account

Response:
{
  "accountId": "acct_1234567890"
}
```

This creates a Stripe Express account and stores the ID on the Tenant:

- `Tenant.StripeConnectedAccountId`
- `Tenant.ConnectStatus` = Pending

### 2. Get Onboarding Link

```http
GET /stripe/connect/onboarding-link?returnUrl=...&refreshUrl=...

Response:
{
  "url": "https://connect.stripe.com/setup/..."
}
```

Redirect the user to this URL to complete Stripe's hosted onboarding.

### 3. Check Status

```http
GET /stripe/connect/status

Response:
{
  "isConnected": true,
  "status": "Active",  // NotConnected, Pending, Active, Restricted, Disabled
  "chargesEnabled": true,
  "payoutsEnabled": true,
  "accountId": "acct_1234567890"
}
```

## Connect Status States

| Status | Description |
| ------ | ----------- |
| NotConnected | No Stripe account created |
| Pending | Account created, onboarding incomplete |
| Active | Fully onboarded, can receive payments |
| Restricted | Limited functionality (verification needed) |
| Disabled | Account disabled |

## Processing Payments

### Public Payment Link Flow

```http
POST /public/payments/{tenantId}/{token}/pay
{
  "paymentMethodId": "pm_1234567890",
  "amount": 500.00  // optional, defaults to full amount due
}

Response:
{
  "paymentIntentId": "pi_1234567890",
  "clientSecret": "pi_..._secret_...",
  "status": "requires_action"  // or "succeeded"
}
```

### Destination Charges

The payment intent is created with:

```csharp
var options = new PaymentIntentCreateOptions
{
    Amount = amountInCents,
    Currency = "usd",
    PaymentMethod = paymentMethodId,
    Confirm = true,
    TransferData = new PaymentIntentTransferDataOptions
    {
        Destination = connectedAccountId
    },
    ApplicationFeeAmount = applicationFeeInCents  // platform fee
};
```

## Webhook Events

The platform handles these Connect-specific webhook events:

| Event | Action |
| ----- | ------ |
| `account.updated` | Sync tenant's ConnectStatus |
| `capability.updated` | Update ChargesEnabled/PayoutsEnabled |

## Testing

### Test Mode

Use Stripe test mode for development:

1. Create test connected accounts
2. Use test card numbers (4242 4242 4242 4242)
3. Simulate onboarding completion

### Test Account Numbers

For testing ACH bank account payments:

- Routing: `110000000`
- Account: `000123456789`

## Configuration

Required environment variables:

```env
Stripe__SecretKey=sk_test_...
Stripe__WebhookSecret=whsec_...
CustomerPortal__BaseUrl=https://portal.example.com
```

## Related Files

- Interface: `src/Core/Logistics.Application/Services/Stripe/IStripeConnectService.cs`
- Implementation: `src/Core/Logistics.Infrastructure/Services/Stripe/StripeConnectService.cs`
- Commands: `src/Core/Logistics.Application/Commands/StripeConnect/`
- Queries: `src/Core/Logistics.Application/Queries/StripeConnect/`
- API: `src/Presentation/Logistics.API/Controllers/StripeConnectController.cs`
- Tenant fields: `StripeConnectedAccountId`, `ConnectStatus`, `ChargesEnabled`, `PayoutsEnabled`
