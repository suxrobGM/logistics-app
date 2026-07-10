---
name: signal-forms-migration
description: "Convert Angular Reactive Forms (FormGroup/FormControl/FormArray) or template-driven forms (ngModel/FormsModule) to the new Signal Forms API (form()/FormField). Handles validation, arrays, nested objects, disabled state, and third-party component bindings."
---

# Signal Forms Migration

Convert an Angular component from Reactive Forms or template-driven forms to Signal Forms (`@angular/forms/signals`).

## Prerequisites

- Angular 22+ — Signal Forms are **stable** as of Angular 22 (this repo runs 22.0.6). The experimental v21 API differs; see the API-detail notes below.
- The component must be identified by the user (file path or component name)

## Step-by-step process

### 1. Read and analyze the component

Read both the `.ts` and `.html` files completely. Identify:

- **Form type**: Reactive (`FormGroup`, `FormControl`, `FormArray`, `ReactiveFormsModule`) or template-driven (`ngModel`, `FormsModule`)
- **Form structure**: All fields, their types, and nesting depth
- **Validation rules**: `Validators.required`, `Validators.email`, `Validators.minLength`, `Validators.maxLength`, `Validators.pattern`, custom validators
- **Dynamic behavior**: Conditional validators, disabled state toggling (`control.enable()`/`control.disable()`)
- **Arrays**: `FormArray` usage, dynamic add/remove of items
- **Template bindings**: `formControlName`, `formGroupName`, `formArrayName`, `[formControl]`, `[(ngModel)]`
- **Third-party components**: PrimeNG, Material, or other UI library form controls
- **Value access patterns**: `.value`, `.getRawValue()`, `.valueChanges`, `.statusChanges`, `.patchValue()`, `.setValue()`, `.reset()`
- **Validation display**: How errors are shown in the template (`.hasError()`, `.errors`, `.touched`, `.dirty`, `.invalid`)
- **Cross-field validation**: Validators applied to groups rather than individual controls

### 2. Plan the migration

Map each element to its signal forms equivalent:

#### Imports

| Before                                                                                                | After                                                                                                                |
| ----------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| `import { ReactiveFormsModule, FormGroup, FormControl, FormArray, Validators } from '@angular/forms'` | `import { form, FormField, required, email, minLength, maxLength, pattern, min, max } from '@angular/forms/signals'` |
| `import { FormsModule } from '@angular/forms'`                                                        | `import { form, FormField } from '@angular/forms/signals'`                                                           |

#### Component imports array

| Before                | After       |
| --------------------- | ----------- |
| `ReactiveFormsModule` | `FormField` |
| `FormsModule`         | `FormField` |

#### Form model

| Before (Reactive)                              | Before (Template)                | After (Signal)                               |
| ---------------------------------------------- | -------------------------------- | -------------------------------------------- |
| `new FormGroup({ name: new FormControl('') })` | Class properties + `[(ngModel)]` | `signal({ name: '' })` + `form(this.model)`  |
| `new FormControl('', Validators.required)`     | N/A                              | Schema function: `required(schemaPath.name)` |
| `new FormArray([...])`                         | N/A                              | `signal({ items: [{ ... }] })`               |

#### Template bindings

| Before                    | After                                                |
| ------------------------- | ---------------------------------------------------- |
| `[formGroup]="form"`      | Remove entirely (no form-level directive needed)     |
| `formControlName="name"`  | `[formField]="myForm.name"`                          |
| `[formControl]="control"` | `[formField]="myForm.fieldName"`                     |
| `formGroupName="address"` | Access nested: `[formField]="myForm.address.street"` |
| `formArrayName="items"`   | Access by index: `myForm.items[i].field`             |
| `[(ngModel)]="property"`  | `[formField]="myForm.property"`                      |

#### Validation display

| Before                                   | After                     |
| ---------------------------------------- | ------------------------- |
| `form.get('name')?.hasError('required')` | `myForm.name().invalid()` |
| `form.get('name')?.touched`              | `myForm.name().touched()` |
| `form.get('name')?.dirty`                | `myForm.name().dirty()`   |
| `form.get('name')?.errors`               | `myForm.name().errors()`  |
| `form.get('name')?.valid`                | `myForm.name().valid()`   |
| `form.invalid`                           | `myForm().invalid()`      |
| `form.valid`                             | `myForm().valid()`        |

#### Value access

| Before                           | After                                                     |
| -------------------------------- | --------------------------------------------------------- |
| `form.value`                     | `model()` (read the signal directly)                      |
| `form.getRawValue()`             | `model()`                                                 |
| `form.patchValue({ name: 'x' })` | `model.update(v => ({ ...v, name: 'x' }))`                |
| `form.setValue(...)`             | `model.set(...)`                                          |
| `form.reset()`                   | `model.set(initialValue)`                                 |
| `form.valueChanges`              | Use `effect()` or `computed()` on the signal              |
| `form.statusChanges`             | `myForm().valid()` / `myForm().invalid()` in `computed()` |

#### Validation rules

| Before (Reactive)           | After (Signal)                                         |
| --------------------------- | ------------------------------------------------------ |
| `Validators.required`       | `required(schemaPath.field, { message: '...' })`       |
| `Validators.email`          | `email(schemaPath.field, { message: '...' })`          |
| `Validators.minLength(n)`   | `minLength(schemaPath.field, n, { message: '...' })`   |
| `Validators.maxLength(n)`   | `maxLength(schemaPath.field, n, { message: '...' })`   |
| `Validators.min(n)`         | `min(schemaPath.field, n, { message: '...' })`         |
| `Validators.max(n)`         | `max(schemaPath.field, n, { message: '...' })`         |
| `Validators.pattern(regex)` | `pattern(schemaPath.field, regex, { message: '...' })` |
| Custom sync validator       | Use `validate()` rule (see below)                      |
| Custom async validator      | Use `validateAsync()` rule (see below)                 |

#### Disabled state

| Before                                           | After                                                                                   |
| ------------------------------------------------ | --------------------------------------------------------------------------------------- |
| `control.disable()` / `control.enable()`         | `disabled(schemaPath.field, { when: () => someSignal() })` (declarative, signal-driven) |
| `new FormControl({ value: '', disabled: true })` | `disabled(schemaPath.field, { when: () => true })` in schema function                   |

> Angular 22: `disabled()`, `readonly()`, and `hidden()` take a config object `{ when: LogicFn }`. The positional-logic overload (`disabled(path, () => ...)`) is deprecated.

### 3. Handle edge cases

#### Third-party UI components (PrimeNG, Material, etc.)

> **Verified facts.** Everything below is proven by the committed probe spec
> `projects/shared/src/lib/forms/signal-forms-compat-probe.spec.ts` (9 tests, passing on
> Angular 22.0.6 + primeng 21.1.6). CI runs it (`bun run ng test shared --watch=false`), so if
> these facts drift the build fails. Treat that spec as the source of truth — not this doc.

**Never put `[formField]` directly on a PrimeNG component.** Two independent, verified failure modes:

- **`pTextarea` crashes at runtime.** PrimeNG's Textarea subscribes to `ngControl.valueChanges` in
  `onInit`, but Signal Forms provides `NgControl` as an `InteropNgControl` that has NO
  `valueChanges` / `statusChanges` — so `undefined.subscribe(...)` throws on the first change
  detection. Across all of primeng's `fesm2022`, Textarea is the ONLY component that does this.
- **`pattern` type collision → TS2322 under `strictTemplates`.** Signal Forms binds the `pattern`
  state input as `readonly RegExp[]`; PrimeNG's `BaseInput` declares `pattern: string`. Six
  components extend `BaseInput` and inherit the collision: **Select, InputNumber, DatePicker,
  AutoComplete, InputMask, Password**.

Components that do NOT extend `BaseInput` and do not subscribe to `valueChanges` (`pInputText`,
`p-checkbox`, `p-toggleswitch`, `p-multiselect`, `p-selectbutton`) happen to work — but relying on
that is fragile. PrimeNG is archived and frozen; do not build on the accident.

**The correct pattern: a wrapper implementing `FormValueControl` only.** Angular 22 bridges custom
Signal Form controls into legacy Reactive and Template-Driven forms automatically — angular.dev:
_"Custom Signal Form Controls can be used with Signal, Reactive and Template-Driven Forms without
any extra compatibility code."_ The probe verifies this (claim B): the SAME `FormValueControl`-only
component two-way syncs under both `[formField]` and `formControlName`. Therefore:

- **NEVER implement `ControlValueAccessor` alongside `FormValueControl`.** No dual-interface
  components, no `NG_VALUE_ACCESSOR` providers. The minimum contract is just `value = model<T>()`.
- For a bare native control, put `[formField]` on the native element — always safe.
- For anything richer, write a thin `FormValueControl` wrapper around the PrimeNG component (or its
  spartan/ui replacement) and bind `[formField]` to the wrapper, never to PrimeNG directly.

**Wrapper-first mapping** (native `<textarea>` for textarea; `FormValueControl` wrappers elsewhere):

- `p-textarea` → native `<textarea>` with `[formField]` (drop `pTextarea` — it crashes; see above)
- `pInputText` → native `<input pInputText>` with `[formField]` (the directive sits on a native input, so this is safe)
- `p-select` / `p-dropdown` → `FormValueControl` wrapper with `[formField]`
- `p-inputNumber` → `FormValueControl` wrapper with `[formField]`
- `p-calendar` / `p-datePicker` → `FormValueControl` wrapper with `[formField]`
- `p-autoComplete` / `p-inputMask` / `p-password` → `FormValueControl` wrapper with `[formField]`
- `p-checkbox` → native `<input type="checkbox">` with `[formField]`, or a `FormCheckboxControl` wrapper

**Do NOT use `compatForm` or `SignalFormControl`** from `@angular/forms/signals/compat`. This
project's migration forbids shims — convert forms fully; do not wrap legacy `FormControl` /
`FormGroup` instances.

#### FormArray / Dynamic arrays

There is **no `FormArray` class** in Signal Forms. Dynamic arrays are plain arrays inside the model
signal; iterate the `FieldTree` with `@for` in the template.

```typescript
// Before
items = new FormArray([new FormGroup({ name: new FormControl('') })]);
addItem() { this.items.push(new FormGroup({ name: new FormControl('') })); }

// After
model = signal({ items: [{ name: '' }] });
myForm = form(this.model);
addItem() {
  this.model.update(v => ({ ...v, items: [...v.items, { name: '' }] }));
}
removeItem(index: number) {
  this.model.update(v => ({ ...v, items: v.items.filter((_, i) => i !== index) }));
}
// Template: myForm.items[i].name
```

#### Cross-field validation

```typescript
// Before: group-level validator
new FormGroup(
  { password: new FormControl(""), confirm: new FormControl("") },
  {
    validators: passwordMatchValidator,
  },
);

// After: use validate() on one field referencing another
form(this.model, (schemaPath) => {
  validate(schemaPath.confirm, () => {
    const m = this.model();
    return m.password === m.confirm
      ? null
      : { kind: "passwordMismatch", message: "Passwords must match" };
  });
});
```

#### No compat shims — convert fully

**Do NOT reach for `compatForm` or `SignalFormControl` from `@angular/forms/signals/compat`.** This
project's migration forbids shims: never wrap existing `FormControl` / `FormGroup` instances inside a
signal form for "gradual migration". Port the form fully — model signal + `form()` + schema rules —
including custom and async validators (`validate()` / `validateAsync()`).

#### Conditional validation with applyWhen

```typescript
form(this.model, (schemaPath) => {
  applyWhen(
    schemaPath.companyName,
    () => this.isBusinessAccount(),
    (p) => {
      required(p, { message: "Company name is required for business accounts" });
    },
  );
});
```

### 4. Implement the migration

1. **Update imports** in the `.ts` file
2. **Replace form creation** with `signal()` + `form()`
3. **Move validation rules** to the schema function
4. **Update template bindings** from `formControlName`/`ngModel` to `[formField]`
5. **Update validation display** in the template
6. **Update value access** patterns in the component class
7. **Remove old imports** (`ReactiveFormsModule`, `FormsModule`, `Validators`, etc.)
8. **Remove old form artifacts** (`FormGroup`, `FormControl`, `FormArray` declarations)

### 5. Verify

- Check for IDE diagnostics / type errors in the template
- Ensure all form fields are bound
- Verify validation rules are equivalent
- Confirm submit handler reads from `model()` instead of `form.value`
- Check that disabled state logic is preserved

## Important notes

- Signal Forms are **stable** as of Angular 22 (this repo runs 22.0.6) — no experimental caveat applies
- `[formField]` on native elements is the most reliable binding method
- Never mix `ReactiveFormsModule`/`FormsModule` directives with `FormField` on the same control
- The `form()` function returns a field tree, not a signal — call `myForm()` to get the root state signal, and `myForm.field()` to get a specific field's state
- Array fields are accessed by index: `myForm.items[0].name` — the form tree automatically tracks array mutations. There is **no `FormArray` class**
- Always add `novalidate` to `<form>` elements to prevent browser validation from conflicting

### Angular 22 API details (correct any v21-era assumptions)

- `disabled()`, `readonly()`, `hidden()` take a config object `{ when: LogicFn }` — the positional-logic overload is deprecated
- `touched` on a custom control is now a **`touched` input plus a `touch()` output** — the old `touched` model was split into the two
- `min` / `max` validators no longer accept string values (numbers only)
- There is **no official migration schematic** from Reactive Forms to Signal Forms — migrate by hand
