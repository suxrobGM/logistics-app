---
paths:
  - "**/*.cs"
---

# Security Rules

## Input Validation

- Validate all user inputs with FluentValidation
- Never trust client-side validation alone
- Sanitize file uploads (check extensions, content type, size)
- Use parameterized queries (EF Core does this by default)

## Authentication & Authorization

- Every controller action must have `[Authorize]` attribute
- Use policy-based authorization with `Permission` constants
- Validate tenant context for all tenant-scoped operations
- Never expose user IDs or sensitive data in URLs

## Secrets Management

- Never hardcode secrets, API keys, or connection strings
- Use `appsettings.json` / environment variables / Azure Key Vault
- Never log sensitive information (passwords, tokens, PII)
- Add `.env` files to `.gitignore`

## Data Protection

- Use HTTPS for all endpoints (enforced in production)
- Implement proper CORS policies
- Hash passwords with secure algorithms (handled by Identity)
- Encrypt sensitive data at rest when required

## File Handling

- Validate file extensions against allowlist
- Check file content matches declared MIME type
- Set reasonable size limits with `[RequestSizeLimit]`
- Store files in Azure Blob Storage, not local filesystem

## SQL Injection Prevention

- EF Core parameterizes queries automatically
- Never use `FromSqlRaw` with string concatenation
- If raw SQL needed, use `FromSqlInterpolated`

## Webhook Security

- Validate webhook signatures for Stripe, ELD providers
- Use constant-time comparison for signature validation
- Log webhook events for audit trail
