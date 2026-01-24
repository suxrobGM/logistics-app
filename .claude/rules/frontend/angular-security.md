---
paths:
  - "src/Client/Logistics.Angular/**/*.ts"
  - "src/Client/Logistics.Angular/**/*.html"
---

# Angular Security Rules

## XSS Prevention

- Never use `[innerHTML]` with user-provided content
- If dynamic HTML is required, use Angular's `DomSanitizer`
- Prefer text interpolation `{{ value }}` over HTML binding

## Template Security

- Never use `bypassSecurityTrustHtml()` unless absolutely necessary
- Validate and sanitize any content before marking as trusted
- Use Angular's built-in sanitization for URLs and styles

## Authentication

- All API calls must include authentication headers (handled by interceptor)
- Check user permissions before showing sensitive UI elements
- Use route guards for protected routes

## Data Handling

- Never store sensitive data in localStorage/sessionStorage
- Use HttpOnly cookies for tokens (handled by backend)
- Clear sensitive data on logout

## Form Security

- Validate all form inputs on client AND server
- Use reactive forms with proper validators
- Sanitize file uploads before sending to API
