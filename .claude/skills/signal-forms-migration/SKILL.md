---
name: signal-forms-migration
description: "Convert Angular Reactive Forms (FormGroup/FormControl/FormArray) or template-driven forms (ngModel/FormsModule) to the Signal Forms API (form()/FormField/FormRoot/submit()) on Angular 22. Handles validation, arrays, nested objects, disabled state, submission, and third-party component bindings."
---

# Signal Forms Migration

Convert an Angular component from Reactive Forms or template-driven forms to Signal Forms
(`@angular/forms/signals`).

## Provenance of everything below

Signal Forms are **stable** as of Angular 22 (this repo runs **22.0.6**). The v21 _experimental_ API
differs substantially — ignore any v21-era blog post, and distrust any answer that mentions a
`[control]` directive, `customError()`, or `FormArray`.

Every claim here is pinned by an executable spec that CI runs:

| Spec                                                                 | Pins                                                                                                                                                                                           |
| -------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `projects/shared/src/lib/ui/form/signal-forms-v22-api-probe.spec.ts` | The Signal Forms API itself (claims F–L), including every example below                                                                                                                        |
| `projects/shared/src/lib/ui/form/form-value-control.spec.ts`         | The `FormValueControl` contract every `ui-*-field` rests on: two-way sync with `[formField]` and no value-accessor glue; `InteropNgControl.errors` is a **keyed object**; 3 known Angular bugs |

**Those specs are the source of truth, not this doc.** If they disagree, the spec wins and this file
is stale. Run them with `bun run ng test shared --watch=false`.

> `form-value-control.spec.ts` is the surviving half of a former `signal-forms-compat-probe.spec.ts`.
> The half that pinned PrimeNG↔Signal-Forms interop was deleted with PrimeNG itself; the half that
> pins **Angular's own** behaviour was kept, because `ui-form-field` renders every inline error in the
> app by reading `InteropNgControl.errors` as a keyed object. If that shape ever flips to an array,
> field errors silently stop rendering everywhere.

## Step-by-step process

### 1. Read and analyze the component

Read both the `.ts` and `.html` files completely. Identify:

- **Form type**: Reactive (`FormGroup`, `FormControl`, `ReactiveFormsModule`) or template-driven (`ngModel`, `FormsModule`)
- **Form structure**: fields, types, nesting depth
- **Validation rules**: `Validators.*`, custom sync/async validators, group-level (cross-field) validators
- **Dynamic behavior**: conditional validators, `control.enable()` / `control.disable()`
- **Arrays**: `FormArray` usage, dynamic add/remove
- **Template bindings**: `formControlName`, `formGroupName`, `formArrayName`, `[formControl]`, `[(ngModel)]`
- **Third-party controls**: our `ui-*-field` wrappers / third-party libraries / custom
- **Value access**: `.value`, `.getRawValue()`, `.valueChanges`, `.statusChanges`, `.patchValue()`, `.setValue()`, `.reset()`
- **Validation display**: `.hasError()`, `.errors`, `.touched`, `.dirty`, `.invalid`
- **Submission**: `(ngSubmit)`, `markAllAsTouched()`, server-error plumbing

### 2. Plan the migration

#### Imports

| Before                                                                                   | After                                                                                                       |
| ---------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| `import {ReactiveFormsModule, FormGroup, FormControl, Validators} from '@angular/forms'` | `import {form, FormField, FormRoot, submit, required, email, minLength, ...} from '@angular/forms/signals'` |
| `import {FormsModule} from '@angular/forms'`                                             | `import {form, FormField} from '@angular/forms/signals'`                                                    |

Component `imports` array: `ReactiveFormsModule` / `FormsModule` → **`FormField`** (per control) and
**`FormRoot`** (on the `<form>` element, if you use native submission).

#### Form model

| Before (Reactive)                            | Before (Template)           | After (Signal)                             |
| -------------------------------------------- | --------------------------- | ------------------------------------------ |
| `new FormGroup({name: new FormControl('')})` | class props + `[(ngModel)]` | `signal({name: ''})` + `form(this.model)`  |
| `new FormControl('', Validators.required)`   | n/a                         | schema function: `required(p.name)`        |
| `new FormArray([...])`                       | n/a                         | `signal({items: [{...}]})` — a plain array |

> **`form()` calls `inject()`.** It must run in an injection context — a field initializer or
> constructor is fine. Outside one, pass `{injector}` (or use `runInInjectionContext`). Building a
> form inside a plain method throws **NG0203**.

#### Template bindings

| Before                    | After                                                  |
| ------------------------- | ------------------------------------------------------ |
| `[formGroup]="form"`      | `[formRoot]="form"` on `<form>` (see submission below) |
| `formControlName="name"`  | `[formField]="form.name"`                              |
| `[formControl]="control"` | `[formField]="form.fieldName"`                         |
| `formGroupName="address"` | nested access: `[formField]="form.address.street"`     |
| `formArrayName="items"`   | index access: `form.items[i].field`                    |
| `[(ngModel)]="prop"`      | `[formField]="form.prop"`                              |

#### Validation display

| Before                                   | After                                                           |
| ---------------------------------------- | --------------------------------------------------------------- |
| `form.get('name')?.hasError('required')` | `form.name().invalid()` (or `form.name().getError('required')`) |
| `form.get('name')?.touched`              | `form.name().touched()`                                         |
| `form.get('name')?.dirty`                | `form.name().dirty()`                                           |
| `form.get('name')?.errors`               | `form.name().errors()` — an **array**, see below                |
| `form.get('name')?.valid`                | `form.name().valid()`                                           |
| `form.invalid`                           | `form().invalid()`                                              |

#### Value access

| Before                              | After                                                     |
| ----------------------------------- | --------------------------------------------------------- |
| `form.value` / `form.getRawValue()` | `model()` or `form().value()`                             |
| `form.patchValue({name: 'x'})`      | `form.name().value.set('x')`                              |
| `form.setValue(v)`                  | `model.set(v)` or `form().value.set(v)`                   |
| `form.reset()`                      | `form().reset()` — **clears touched/dirty only**          |
| `form.reset(initial)`               | `form().reset(initial)` — clears state **and** sets value |
| `form.valueChanges`                 | `effect()` / `computed()` / `linkedSignal()` on the model |
| `form.statusChanges`                | `computed(() => form().valid())`                          |

> `form()` does **not** copy the model — it wraps it. `form.x().value.set(...)` writes straight
> through to the model signal, and vice versa.

#### Validation rules

| Before (Reactive)         | After (Signal)                                                                |
| ------------------------- | ----------------------------------------------------------------------------- |
| `Validators.required`     | `required(p.field, {message: '...'})`                                         |
| `Validators.email`        | `email(p.field, {message: '...'})`                                            |
| `Validators.minLength(n)` | `minLength(p.field, n, {message: '...'})`                                     |
| `Validators.maxLength(n)` | `maxLength(p.field, n, {message: '...'})`                                     |
| `Validators.min(n)`       | `min(p.field, n, ...)` — **numbers only**                                     |
| `Validators.max(n)`       | `max(p.field, n, ...)` — **numbers only**                                     |
| `Validators.min(date)`    | `minDate(p.field, d, ...)` / `maxDate(p.field, d, ...)`                       |
| `Validators.pattern(re)`  | `pattern(p.field, /re/, {message: '...'})` — takes a **RegExp**, not a string |
| custom sync validator     | `validate()`                                                                  |
| custom async validator    | `validateAsync()` / `validateHttp()`                                          |
| group-level validator     | `validate()` on a field, or `validateTree()`                                  |

Every validator's config is `{message}` **XOR** `{error}`, plus an optional
`when: ({value, valueOf}) => boolean` that makes the rule conditional.

#### Disabled / readonly / hidden

| Before                                         | After                                           |
| ---------------------------------------------- | ----------------------------------------------- |
| `control.disable()` / `.enable()`              | `disabled(p.field, {when: ({valueOf}) => ...})` |
| `new FormControl({value: '', disabled: true})` | `disabled(p.field, {when: () => true})`         |

`disabled()`, `readonly()`, and `hidden()` take a config object `{when: LogicFn}`. The positional
overload (`disabled(p.x, () => ...)`) still compiles but is **`@deprecated`** — do not use it.

### 3. Submission — the part with no Reactive Forms analogue

`submit()` does far more than an `(ngSubmit)` handler. Verified behavior:

1. **Marks the whole tree touched** — _before_ checking validity, and including already-valid fields.
   Inline errors reveal themselves with no `markAllAsTouched()` call.
2. **Skips `action` when the form is invalid**, running `onInvalid` instead. Resolves `false`.
3. **Guards re-entrancy** — a second `submit()` while one is in flight returns `false` immediately.
4. **Drives `form().submitting()`** for the duration of `action`.
5. **Maps errors returned by `action` back onto individual fields** — this is how server-side
   validation errors land on the right control, with no bespoke plumbing.
6. Throws **NG1915** if no action is supplied.

The clean shape — declare `submission` on the form, then let `[formRoot]` wire the `<form>`:

```ts
readonly model = signal({username: '', email: ''});
readonly form = form(
  this.model,
  (p) => {
    required(p.username, {message: 'Username is required'});
    email(p.email, {message: 'Enter a valid email address'});
  },
  {
    submission: {
      action: async (root) => {
        const res = await this.api.save(this.model());
        // Return errors to attach them to fields; return undefined for success.
        return res.ok
          ? undefined
          : [{kind: 'server', message: res.error, fieldTree: root.username}];
      },
    },
  },
);
```

```html
<form [formRoot]="form">
  <input [formField]="form.username" />
  <button type="submit" [disabled]="form().submitting()">Save</button>
</form>
```

`<form [formRoot]>` **sets `novalidate` itself** and calls `submit()` on the submit event (only when
the form declares `submission` options). Do not add `novalidate` by hand, and do not add `(ngSubmit)`.

To submit imperatively instead, call `submit(this.form, async () => {...})` — same semantics, no
`[formRoot]` needed.

### 4. Handle edge cases

#### Errors are an array of `{kind, message}`, and the kinds are camelCase

`field().errors()` returns `ValidationError[]`, **not** the classic `ValidationErrors` object. The
built-in `kind`s do **not** all match the legacy reactive-forms keys:

| Legacy reactive key | Signal Forms `kind` | payload prop                             |
| ------------------- | ------------------- | ---------------------------------------- |
| `required`          | `required`          | —                                        |
| `email`             | `email`             | —                                        |
| `min` / `max`       | `min` / `max`       | `.min` / `.max`                          |
| **`minlength`**     | **`minLength`**     | **`.minLength`** (not `.requiredLength`) |
| **`maxlength`**     | **`maxLength`**     | **`.maxLength`** (not `.requiredLength`) |
| `pattern`           | `pattern`           | `.pattern`                               |

This is a **silent** trap. A template checking `errors['minlength']?.requiredLength` compiles, runs,
marks the field invalid — and renders nothing, or falls through to a generic message. It bites
through `InteropNgControl.errors` too, which keys by the raw `kind`.

So don't switch on kinds at all. Render the message the rule already carries:

```html
@if (form.email().touched() && form.email().invalid()) { @for (error of form.email().errors(); track
error) {
<p class="error">{{ error.message }}</p>
} }
```

…which means **every validator call should pass `{message: '...'}`**.

#### No `ng-invalid` / `ng-touched` / `ng-dirty` classes

Signal Forms applies **no status classes at all** by default. Any CSS rule or
`querySelector('.ng-invalid')` silently stops matching. Opt in explicitly if you need them:

```ts
provideSignalFormsConfig({
  classes: {
    "ng-invalid": (b) => b.state().invalid(),
    "ng-touched": (b) => b.state().touched(),
    "ng-dirty": (b) => b.state().dirty(),
  },
});
```

#### Third-party and custom UI controls

**Never put `[formField]` directly on a third-party component.** A control written for Reactive Forms
implements `ControlValueAccessor`, and two failure modes follow from that:

- **Runtime crash on `ngControl` observables.** Such controls commonly subscribe to
  `ngControl.valueChanges` / `statusChanges` in `ngOnInit`. Signal Forms provides `NgControl` as an
  `InteropNgControl` that has **neither** — so `undefined.subscribe(...)` throws on the first change
  detection.
- **State-input type collisions → TS2322 under `strictTemplates`.** Signal Forms binds its state
  inputs onto the bound element, and the names are reserved (see below). A library control that
  declares an input of the same name with a different type fails to compile — e.g. Signal Forms binds
  `pattern` as `readonly RegExp[]`, so any control declaring `pattern: string` collides.

Both were observed for real against PrimeNG before it was removed (a `pTextarea` + `[formField]`
crashed on the missing `valueChanges`; `BaseInput.pattern: string` hit the TS2322). The repo no
longer depends on that library, so those specific probes are gone — but the **mechanism is Angular's
`InteropNgControl`, not PrimeNG's**, and it will bite the next `ControlValueAccessor`-based library
you reach for in exactly the same way.

**The correct pattern: a wrapper implementing `FormValueControl` only.** Angular 22 bridges custom
Signal Form controls into legacy Reactive and Template-Driven forms automatically — angular.dev:
_"Custom Signal Form Controls can be used with Signal, Reactive and Template-Driven Forms without any
extra compatibility code."_ Therefore:

- **Never implement `ControlValueAccessor` alongside `FormValueControl`.** No dual-interface
  components, no `NG_VALUE_ACCESSOR`. The minimum contract is just `value = model<T>()`.
- For a bare native control, put `[formField]` on the native element — always safe.
- For anything richer, wrap it and bind `[formField]` to the wrapper, never to the library component.

In **this repo** the wrappers already exist (`ui-text-field`, `ui-select-field`, `ui-date-field`, …
in `projects/shared/src/lib/ui/form/`, exported from `@logistics/shared/ui`). They wrap the vendored
spartan/ui Helm primitives, and they are the only thing `[formField]` should bind to. Templates
already speak `ui-*-field`, so a form migration is normally a **`.ts`-only change**. If a control
isn't wrapped yet, wrap it first.

##### `[formField]` value types are invariant

A control's `value` is a `ModelSignal<T>` — read **and** write — so `T` is invariant. The model
field's type must equal the wrapper's `T` **exactly**:

```ts
// ui-text-field is FormValueControl<string>
interface Model {
  name: string | null;
} // ✗ TS2322: 'string | null' is not assignable to 'string'
interface Model {
  name: string;
} // ✓
```

Neither direction is allowed: a `FieldTree<string>` will not bind a `FormValueControl<string | null>`
either. This is the single most common error when converting a form, because reactive `FormControl`s
were routinely typed `string | null` while the DTO field is optional.

The convention in this repo:

| Wrapper              | `T`              | Model field      | Empty value |
| -------------------- | ---------------- | ---------------- | ----------- |
| `ui-text-field`      | `string`         | `string`         | `""`        |
| `ui-textarea-field`  | `string`         | `string`         | `""`        |
| `ui-number-field`    | `number \| null` | `number \| null` | `null`      |
| `ui-date-field`      | `Date \| null`   | `Date \| null`   | `null`      |
| `ui-checkbox-field`  | `boolean`        | `boolean`        | `false`     |
| `ui-select-field<T>` | `T \| null`      | `T \| null`      | `null`      |

So an optional text field holds `""`, not `null`. **Coerce at the boundaries**: `dto.x ?? ""` coming
in, and `v.x || null` (or `|| undefined`) going out — otherwise you start sending `""` to an API that
previously received `null`.

Note this also means `pattern()` / `minLength()` / `maxLength()` now typecheck against a `string`
path. An _optional_ field must guard them, since reactive `Validators.pattern` skipped empty values:

```ts
pattern(p.unNumber, /^UN\d{4}$/i, {
  when: ({ valueOf }) => valueOf(p.unNumber).length > 0,
  message: "Enter a UN number such as UN1203.",
});
```

##### Optional control hooks, and reserved input names

`FormUiControl` declares optional `focus?(options?)` and `reset?()` methods; `focusBoundControl()`
calls them. A wrapper whose host is a non-focusable custom element **must** implement `focus()` to
delegate to its inner input — otherwise "focus the first invalid field" silently does nothing.

State inputs a control may opt into: `errors`, `disabled`, `disabledReasons`, `readonly`, `hidden`,
`invalid`, `pending`, `touched`, `dirty`, `name`, `required`, `min`, `minLength`, `max`, `maxLength`,
`pattern` — plus a `touch` output. **These names are reserved.** A wrapper input sharing one gets
silently auto-bound by Signal Forms; TS2416 usually catches it (e.g. `ui-autocomplete-field`'s search
threshold had to be renamed `minQueryLength`).

#### FormArray / dynamic arrays

There is **no `FormArray` class**. Arrays are plain arrays inside the model signal; index the field
tree directly, and use `applyEach` for a per-item schema.

```ts
readonly model = signal({items: [{name: ''}]});
readonly form = form(this.model, (p) => {
  applyEach(p.items, (item) => required(item.name, {message: 'Name is required'}));
});

addItem() {
  this.model.update((v) => ({...v, items: [...v.items, {name: ''}]}));
}
removeItem(index: number) {
  this.model.update((v) => ({...v, items: v.items.filter((_, i) => i !== index)}));
}
// Template: [formField]="form.items[i].name"
```

#### Cross-field validation

There is **no `customError()` export**. Return a plain `{kind, message}` object.

```ts
form(this.model, (p) => {
  validate(p.confirm, ({ valueOf }) =>
    valueOf(p.password) === valueOf(p.confirm)
      ? null
      : { kind: "passwordMismatch", message: "Passwords must match" },
  );
});
```

Use `validateTree()` when one rule must attach errors to _several_ fields at once.

#### Conditional validation

Two forms — `applyWhen` scopes a whole sub-schema, `{when}` scopes a single rule:

```ts
form(this.model, (p) => {
  applyWhen(
    p,
    ({ value }) => value().isBusiness,
    (bp) => {
      required(bp.companyName, { message: "Company name is required for business accounts" });
      required(bp.taxId);
    },
  );

  // single rule — no applyWhen needed
  required(p.companyName, { when: ({ valueOf }) => valueOf(p.isBusiness) });
});
```

`applyWhenValue()` additionally narrows the value type via a type predicate.

#### No compat shims — convert fully

**Do NOT use `compatForm` or `SignalFormControl` from `@angular/forms/signals/compat`.** This
project's migration forbids shims: never wrap an existing `FormControl` / `FormGroup` inside a signal
form for "gradual migration". Port the form fully — model signal + `form()` + schema rules —
including custom and async validators. If a form resists conversion, fix the form.

### 5. Implement

1. Replace form creation with `signal()` + `form()` (in a field initializer — injection context).
2. Move validation rules into the schema function, each with a `{message}`.
3. Move `disable()` / `enable()` logic into `disabled(p.x, {when})`.
4. Move the submit handler into `submission.action`; delete `markAllAsTouched()`.
5. Update template bindings to `[formField]` / `[formRoot]`.
6. Update error rendering to iterate `errors()` and print `error.message`.
7. Update value access in the class (`model()`, `form.x().value.set()`).
8. Delete `ReactiveFormsModule` / `FormsModule` / `Validators` / `FormGroup` / `FormControl` imports.

### 6. Verify

- No template type errors (`strictTemplates` catches the `pattern` and reserved-name collisions).
- Every field is bound; no orphan fields (**NG01902**).
- Validation rules are equivalent — _including the error messages_, which are now data, not markup.
- Submit reads from `model()`, not `form.value`.
- Disabled logic is preserved.
- Anything that depended on `.ng-invalid` has been re-pointed.

## Angular 22 API reference (as installed)

Exported from `@angular/forms/signals`:

- **Structure**: `form`, `schema`, `apply`, `applyEach`, `applyWhen`, `applyWhenValue`, `submit`
- **Directives**: `FormField` (`[formField]`), `FormRoot` (`form[formRoot]`)
- **Validators**: `required`, `email`, `min`, `max`, `minDate`, `maxDate`, `minLength`, `maxLength`, `pattern`
- **Custom validation**: `validate`, `validateAsync`, `validateHttp`, `validateTree`, `validateStandardSchema`
- **Logic**: `disabled`, `readonly`, `hidden`, `debounce`
- **Errors**: `requiredError`, `emailError`, `minError`, `maxError`, `minDateError`, `maxDateError`, `minLengthError`, `maxLengthError`, `patternError`, `standardSchemaError` (there is **no** `customError`)
- **Types**: `FieldTree`, `FieldState`, `FormValueControl`, `FormCheckboxControl`, `FormUiControl`, `ValidationError`, `LogicFn`, `SchemaPath`
- **Config**: `provideSignalFormsConfig`, `metadata`, `createMetadataKey`, `transformedValue`

`FieldState` members: `value` (writable), `controlValue`, `errors`, `errorSummary` (self +
descendants), `valid`, `invalid`, `pending`, `submitting`, `touched`, `dirty`, `disabled`,
`disabledReasons`, `readonly`, `hidden`, `required`, `min`, `max`, `minLength`, `maxLength`,
`pattern`, `name`, `keyInParent`, `markAsTouched(opts?)`, `markAsDirty()`, `reset(value?)`,
`getError(kind)`, `reloadValidation()`, `focusBoundControl(opts?)`.

Other v22 notes:

- `touched` on a custom control is a **`touched` input plus a `touch()` output** — the old `touched`
  model was split in two.
- `min` / `max` no longer accept strings (numbers only); dates get `minDate` / `maxDate`.
- `pattern()` takes a `RegExp`, not a string.
- There is **no official migration schematic** from Reactive Forms to Signal Forms — migrate by hand.
- `form()` returns a **field tree, not a signal**. Call `form()` for the root state, `form.x()` for a
  field's state.
