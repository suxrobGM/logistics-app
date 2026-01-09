# API Authentication

Logistics TMS uses OAuth 2.0 / OpenID Connect via Duende IdentityServer.

## Identity Server URLs

| Environment | URL |
|-------------|-----|
| Development | <https://localhost:7001> |
| Production | <https://id.yourdomain.com> |

## OpenID Configuration

Discovery document:

```text
GET /.well-known/openid-configuration
```

## Authentication Flow

### 1. Resource Owner Password (Direct Login)

For first-party applications (Office App, Driver App):

```http
POST /connect/token HTTP/1.1
Host: id.yourdomain.com
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=logistics.client
&client_secret=client_secret
&username=user@example.com
&password=password123
&scope=openid profile logistics.api
```

Response:

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "abc123...",
  "scope": "openid profile logistics.api"
}
```

### 2. Authorization Code (Third-Party)

For external integrations:

```text
GET /connect/authorize?
  response_type=code
  &client_id=third_party_app
  &redirect_uri=https://app.example.com/callback
  &scope=openid profile logistics.api
  &state=random_state
```

### 3. Refresh Token

```http
POST /connect/token HTTP/1.1
Host: id.yourdomain.com
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&client_id=logistics.client
&client_secret=client_secret
&refresh_token=abc123...
```

## Using Access Tokens

Include the token in the Authorization header:

```http
GET /api/loads HTTP/1.1
Host: api.yourdomain.com
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Token Claims

Access tokens contain:

| Claim | Description |
|-------|-------------|
| `sub` | User ID |
| `email` | User email |
| `name` | Display name |
| `role` | User role(s) |
| `tenant` | Tenant ID |

Example decoded token:

```json
{
  "sub": "user-123",
  "email": "dispatcher@company.com",
  "name": "John Doe",
  "role": "Dispatcher",
  "tenant": "default",
  "iat": 1704067200,
  "exp": 1704070800
}
```

## Role-Based Authorization

Endpoints check roles:

```csharp
[Authorize(Roles = "Owner,Manager")]
[HttpPost("loads")]
public async Task<IActionResult> CreateLoad(...)
```

### Role Hierarchy

| Role | Permissions |
|------|------------|
| SuperAdmin | System-wide access |
| Owner | Full tenant access |
| Manager | Operational access |
| Dispatcher | Load/trip management |
| Driver | View assigned loads |

## API Configuration

In `appsettings.json`:

```json
{
  "IdentityServer": {
    "Authority": "https://id.yourdomain.com",
    "Audience": "logistics.api",
    "ValidIssuers": [
      "https://id.yourdomain.com",
      "https://localhost:7001"
    ]
  }
}
```

## Client Configuration

### Angular (Office App)

```typescript
// environment.ts
export const environment = {
  identityUrl: 'https://id.yourdomain.com',
  apiUrl: 'https://api.yourdomain.com',
  clientId: 'logistics.office'
};
```

### Mobile App

```kotlin
object AuthConfig {
    const val AUTHORITY = "https://id.yourdomain.com"
    const val CLIENT_ID = "logistics.mobile"
    const val REDIRECT_URI = "logistics://callback"
}
```

## Token Storage

| Platform | Recommended Storage |
|----------|-------------------|
| Browser | Memory (not localStorage) |
| Mobile | Secure storage (Keychain/Keystore) |
| Server | Environment variables |

## Troubleshooting

### Invalid Token

- Check token expiration
- Verify issuer matches configuration
- Ensure audience is correct

### CORS Errors

Verify Identity Server CORS settings include your app's origin.

### Clock Skew

Tokens may fail if server clocks are out of sync. Use NTP:

```bash
sudo ntpdate -u pool.ntp.org
```

## Next Steps

- [API Endpoints](endpoints.md) - Available resources
- [SignalR Hubs](signalr-hubs.md) - Real-time authentication
