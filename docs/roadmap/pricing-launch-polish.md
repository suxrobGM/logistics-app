# Pricing Page & Billing Polish

- **Status**: Planned
- **Priority**: P1 - conventions every competitor pricing page has; cheap to add
- **Effort**: S
- **Category**: Launch hygiene

## Why

Current pricing (Starter $29+$12/truck, Pro $79+$9/truck, Enterprise $169+$6/truck) undercuts
everyone and has smooth tier crossovers (Starter@10=$149 → Pro@10=$169; Pro@30 = Ent@30 = $349 -
keep that property when changing prices). Missing: the standard trust signals around it.

## What to build

- **Annual billing discount** (~2 months free): annual Stripe Prices per plan, toggle on `website/pages/home/sections/pricing/`, handled in `StripeSubscriptionService`/`StripePlanService`.
- **Free trial**: DataTruck advertises one on every plan. 14-day trial via Stripe `trial_period_days`; show "14-day free trial, no credit card" (or with card - decide) on pricing cards.
- **"From $41/mo" framing**: 1 truck on Starter is the lowest credible number in the market - headline it on the website hero/pricing.
- **DVIR/safety gating check**: DVIR is a federal legal requirement but lives in Professional (`SubscriptionPlanSeeder` puts `Safety` in Pro). Consider moving basic DVIR down to Starter, keep driver-behavior analytics + accident management in Pro - avoids "the cheap plan is non-compliant" perception.
- **EUR display** when [eu-market-readiness](eu-market-readiness.md) starts - `Money` VO already supports currency.
- **Comparison page**: `website/pages/compare/` exists - add DataTruck row highlighting "AI included vs. +$399/mo".
- Keep seeder, `pricing.ts`, and Stripe Prices in sync - three sources of truth today; consider generating the website tiers from the API instead of hardcoding.

## Acceptance

Pricing page shows monthly/annual toggle, trial badge, and "from $41/mo"; plan features on the page exactly match `SubscriptionPlanSeeder` reality.

## Notes

_(add dated implementation notes here)_
