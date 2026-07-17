# Broker Credit Check

- **Status**: Done
- **Priority**: P0 — DataTruck's AI Dispatcher vets broker credit before booking; ours books blind
- **Effort**: S
- **Category**: Table stakes / AI tool

## Why

Getting stiffed by a broker is an existential risk for a small carrier. Credit data is available via
DAT (CreditStop Booster), Truckstop (credit scores + days-to-pay), and FMCSA authority status — we
already integrate the first two for load search (`Infrastructure.Integrations.LoadBoard`).

## What to build

- `CheckBrokerCreditTool` in `src/Infrastructure/Logistics.Infrastructure.AI/Tools/` via the `add-dispatch-tool` skill — read tool, snake_case name `check_broker_credit`. Registry entry means both the dispatch agent and MCP server get it for free.
- Backing port `IBrokerCreditService` in `Application.Abstractions`, implemented in `Infrastructure.Integrations.LoadBoard` (pull credit fields from DAT/Truckstop responses; FMCSA SAFER authority lookup as free fallback).
- System prompt rule in `AiDispatchSystemPrompt`: never book a load-board load without a credit check; refuse below tenant-configurable threshold.
- Surface credit score + days-to-pay on `tms-portal/pages/load-board/` listing cards for manual dispatching too.

## Acceptance

`book_loadboard_load` on a broker below the credit threshold is refused with a clear reason; load-board UI shows credit data on every listing that has it.

## Notes

- **2026-07-16**: Shipped on `feat/fuel-integrations`. `check_broker_credit` read tool (registry + `LoadBoardTools` gate → dispatch agent + MCP), `IBrokerCreditService` in `Infrastructure.Integrations.LoadBoard/Credit/` (24h `BrokerCreditRecord` tenant-DB cache → Demo deterministic score → FMCSA QCMobile authority fallback; DAT/Truckstop credit fields mapped from search responses when the subscription includes them). Hard server-side gate in `BookLoadBoardLoadHandler` (`BROKER_CREDIT_BELOW_THRESHOLD` prefix, dispatcher `OverrideCreditCheck` flag, missing score never blocks, inactive authority always blocks) + system-prompt rule. Threshold: `TenantSettings.MinBrokerCreditScore`, editable in company settings. UI: broker column + credit badge on load-board search, override confirm dialog. Also fixed pre-existing bug: search results were never persisted, so booking by listing ID could never succeed — `SearchLoadBoardHandler` now upserts listings.
