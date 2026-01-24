---
name: eld-integration
description: ELD provider integration architecture and patterns
---

# ELD Integration Architecture

The ELD (Electronic Logging Device) integration pulls driver Hours of Service data from external providers for FMCSA compliance.

## Key Components

- `IEldProviderService` - Interface for all ELD operations (in Application layer)
- `IEldProviderFactory` - Factory to get provider by type
- `SamsaraEldService`, `MotiveEldService` - Provider implementations (in Infrastructure layer)
- `EldSyncJob` - Hangfire job that syncs HOS data every 5 minutes (fallback for webhooks)

## Data Flow

1. Configure ELD provider credentials per tenant via API
2. Map TMS employees to external ELD driver IDs
3. Webhooks receive real-time updates, or background job polls every 5 minutes
4. HOS data stored in `DriverHosStatus` entity, displayed in Office App ELD Dashboard

## Related Entities

- `EldProviderConfiguration` - Tenant-specific ELD provider settings
- `DriverHosStatus` - Hours of Service status for drivers
- `EldDriverMapping` - Maps TMS employees to external ELD driver IDs

## Webhook Endpoints

- `/webhooks/eld/samsara` - Samsara webhook receiver
- `/webhooks/eld/motive` - Motive (KeepTruckin) webhook receiver

## Configuration

In `appsettings.json`:

```json
"Eld": {
  "Samsara": { "BaseUrl": "https://api.samsara.com" },
  "Motive": { "BaseUrl": "https://api.keeptruckin.com/v1" }
}
```

## Adding a New ELD Provider

1. Create `{Provider}EldService` implementing `IEldProviderService` in Infrastructure
2. Register in `IEldProviderFactory`
3. Add webhook controller in `Logistics.API/Controllers/Webhooks/`
4. Add configuration section to `appsettings.json`
