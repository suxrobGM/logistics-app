# LogisticsX — Pre-Seed Budget & Use of Funds

**Prepared:** June 2026
**Stage:** Deployed to production; in active test with 1 US trucking company
**Team:** Solo technical founder (full-stack: backend, web portals, mobile app, infra/DevOps) + 1 planned sales hire
**Runway:** 12 months
**Recommended raise:** **$70k** (covers the plan below with a small built-in cushion)

---

## Executive summary

The product is **already built and live** — a full multi-tenant Transportation Management System (TMS)
for trucking companies, shipped solo. This raise does **not** fund product development. It funds
**12 months of runway to convert the current pilot into repeatable, paying revenue** through a
dedicated sales effort.

Total 12-month spend is **~$64k** — an exceptionally lean burn (**~$4,580/month**) for a production
SaaS platform, because the founder does all engineering and the stack is self-hosted on a low-cost VPS.

---

## What's already built (de-risks the technical bet)

| Area              | Delivered                                                                                                                                                                  |
| ----------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Core platform     | Multi-tenant TMS — master DB + isolated database per tenant; .NET (DDD + CQRS), PostgreSQL                                                                                 |
| Web portals       | 4 Angular apps: Admin, TMS (dispatcher), Customer, and marketing Website                                                                                                   |
| Mobile            | Kotlin Multiplatform driver app (Android + iOS)                                                                                                                            |
| AI Dispatch agent | Multi-provider LLM agent (Claude / OpenAI / DeepSeek) with tool-calling, human-in-the-loop & autonomous modes, weekly quota system, and Stripe usage-based overage billing |
| Billing           | Stripe Connect — subscriptions + usage-based billing                                                                                                                       |
| Integrations      | ELD providers (Samsara, Motive, TT ELD), Mapbox, Resend, Firebase push, Cloudflare R2                                                                                      |
| Platform extras   | Self-hosted MCP server, Identity Server (auth), Docker + nginx deploy, GitHub Actions CI/CD                                                                                |

**Takeaway for investors:** the hard, expensive part — building a production-grade, integrated TMS — is
done. Remaining risk is distribution, which this raise directly addresses.

---

## Assumptions

- **No engineering payroll** — the founder builds and operates everything. This eliminates the single
  largest cost typical of a SaaS startup.
- **Self-hosted infrastructure** on a low-cost VPS (~$100/yr) — keeps infra near-zero until tenant count grows.
- **DeepSeek** is the default LLM (extremely low cost-per-token); the $50/mo line is a conservative ceiling.
- **Stripe fees** (~2.9% + 30¢ + Connect fees) come out of payment volume — they scale with revenue and
  are **not** a budgeted cash line.
- **Cybersecurity review** is handled in-kind (no paid vendor at this stage).
- All figures in **USD**.

---

## 1. One-time setup costs (Months 0–2)

| Item                                  | Cost       | Notes                                                                                            |
| ------------------------------------- | ---------- | ------------------------------------------------------------------------------------------------ |
| Mac mini — iOS build machine          | $2,000     | Needed to compile the Kotlin Multiplatform driver app's iOS target (Pro chip + upgraded RAM/SSD) |
| Incorporation (Stripe Atlas / Clerky) | $500       | Delaware C-Corp; Atlas suits a non-US founder                                                    |
| Branding / pitch (DIY)                | $500       | Deck, one-pager, asset polish                                                                    |
| **Subtotal**                          | **$3,000** | Apple Developer account already owned                                                            |

---

## 2. Monthly recurring operating costs

| Category                                               | Monthly         | 12-month    |
| ------------------------------------------------------ | --------------- | ----------- |
| Founder stipend                                        | $2,000          | $24,000     |
| Sales specialist (base)                                | $2,000          | $24,000     |
| Sales enablement & ads (CRM, outreach tools, ad tests) | $500            | $6,000      |
| LLM / AI — DeepSeek (ceiling)                          | $50             | $600        |
| Resend (email)                                         | $20             | $240        |
| VPS + domain                                           | ~$10            | $120        |
| Mapbox                                                 | $0 (free tier)  | $0          |
| **Subtotal**                                           | **$4,580 / mo** | **$54,960** |

> **Sales specialist:** budgeted as a **remote rep doing US outreach at $2,000/mo base + commission**.
> Commission (~10–20% of first-year contract value) is largely **self-funding** from the revenue it
> generates. A US-based rep would run $4,000–6,000/mo base — raise the round accordingly if you go that route.

---

## 3. Periodic costs

| Item                                   | Annual | Notes                                                           |
| -------------------------------------- | ------ | --------------------------------------------------------------- |
| In-person travel + events (1–2 events) | $6,000 | Trade shows / on-site demos — trucking buyers convert in person |

---

## 4. 12-month total

|                            | Amount       |
| -------------------------- | ------------ |
| One-time setup             | $3,000       |
| Monthly recurring (×12)    | $54,960      |
| Periodic (travel + events) | $6,000       |
| **Total 12-month runway**  | **~$63,960** |

**The ask:** raising a clean **$70,000** covers this plan and leaves ~$6k of cushion (there is no separate
contingency line in the base plan). A more comfortable **$80k–90k** would let you ramp the sales rep to full
quota and increase ad spend for the same equity narrative.

---

## 5. Use of funds

| Bucket                                        | Share of ~$64k |
| --------------------------------------------- | -------------- |
| Sales (rep base + enablement + travel/events) | 56%            |
| Founder stipend                               | 38%            |
| One-time setup & hardware                     | 5%             |
| Product infra + AI + email                    | 1.5%           |

**~56% of the round goes directly into sales and distribution** — exactly what a pre-seed investor wants
from a founder who has already built the product and now needs to prove it sells.

---

## 6. Costs that scale with revenue (flag, don't pre-fund)

- **Stripe fees** — a percentage of payment volume; offset by subscription + usage revenue.
- **LLM / infrastructure** — grow only as tenants are added; the $0.20/unit AI overage billing already
  passes premium usage through to tenants.
- **Sales commissions** — funded by the deals they're paid on.

---

## 7. Deferred to the seed round

- **SOC 2 Type II** (~$10k–20k/yr) — once enterprise fleets demand it.
- **Legal** — SAFE review, Terms of Service / Privacy / DPA, trademark (planned once the round closes
  and the entity is formalized).
- **Insurance** — cyber + E&O / professional liability once revenue and customer data volume grow.
- **Second hires** — a second salesperson, then a second engineer, funded once revenue is repeatable.
- **ELD partner certifications** (Samsara / Motive) — budget when those programs are formalized.

---

## 8. Risk notes (for the founder)

- **No contingency buffer** in the base $64k plan — it is a true floor. The recommended $70k raise restores
  ~$6k of slack.
- **$2,000/mo founder stipend is lean** — confirm it's livable for 12 months so you aren't forced into
  contract work that stalls the company.
- **Travel is lumpy** — the $6k events budget lands in specific months, not evenly; see the cash-flow model.

---

## One-line pitch

> _"A fully-built, live multi-tenant TMS — backend, four web portals, a mobile driver app, an AI dispatch
> agent, Stripe billing, and ELD integrations — built solo. I'm not raising to build; I'm raising to sell.
> ~$64k funds 12 months: a modest founder stipend, a dedicated salesperson, and the tools to turn one pilot
> into a repeatable revenue motion. Burn is under $4,600/month."_

---

_This is a planning estimate for internal and fundraising-discussion use, not financial, tax, or legal
advice. Validate incorporation, securities (SAFE), and tax specifics with a qualified professional before
closing a round. A live, adjustable version of these numbers is in `pre-seed-cashflow.csv`._
