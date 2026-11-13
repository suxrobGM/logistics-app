---
paths:
  - "**/*"
---

# Code Quality

## Simple changes

- Make the smallest direct change. Before adding a layer, interface, entity, job or generic mechanism, check whether editing the existing handler or component does it.
- No idempotency keys, retry ledgers, outbox tables, marker interfaces or pipeline behaviours unless asked. Raise bigger designs instead of building them.
- No fallbacks, shims or deprecated-but-kept code. Migrate call sites and delete the old code in the same change.

## Control flow

- One ternary per expression. A second `?` becomes early returns, a `switch` expression or a lookup row.
- If a condition needs line breaks or hides an `await` in a chain, name the parts: `if (numberChanged && await IsNumberTakenAsync(...))`. A multi-step check becomes a method returning the error or null.

## Names

- Types: a plain role noun saying what it does (`BrokerCreditChecker`, `LoadLookupBuilder`). No jargon (`Gate`, `Uniqueness`, `Sentinel`, `Dtos`).
- Methods start with a verb (`GetChatRole`, `CheckAsync`). No `RoleOf(x)` / `Thing.Of(x)`; factories are `Create`.

## Comments

Default to none. Write one only for why: code that looks wrong but is right, a load-bearing order or lifetime, an external contract, a formula's intent.

- One short line; two only for a real constraint. Short but concrete: name the subject and the effect.
- Doc syntax on declarations (`/** */`, `///`, KDoc) so it shows on hover; `//` inside bodies.
- Never section labels, restated names, change history, ticket references or commented-out code. A comment explaining a name means rename.

## File size

A smell, not a verdict: split by responsibility.

| File type | Aim | Refactor at |
| --- | --- | --- |
| C# entity | 100–200 | 250 |
| C# EF configuration | 50–100 | 200 |
| C# handler / controller | 150–250 | 400 |
| C# application service | 200–300 | 500 |
| C# infrastructure service | 250–400 | 600 |
| Angular component `.ts` / React `.tsx` | 150–250 | 350 |
| Angular template | 100–200 | 300 |
| Angular service / signal store | 200–400 | 500 |
| Kotlin class | 150–300 | 400 |

Generated code is exempt. Entities split as `partial` by concern (`Load.Status.cs`); handlers push logic into the entity or a domain service; templates extract child components.
