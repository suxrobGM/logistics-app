---
name: signal-forms-migration
description: "Convert Angular Reactive Forms (FormGroup/FormControl/FormArray) or template-driven forms (ngModel/FormsModule) to the new Signal Forms API (form()/FormField). Handles validation, arrays, nested objects, disabled state, and third-party component bindings."
---

# Signal Forms Migration

Convert an Angular component from Reactive Forms or template-driven forms to Signal Forms (`@angular/forms/signals`).

## Prerequisites

- Angular 21+ (signal forms require v21 minimum)
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

| Before | After |
|--------|-------|
| `import { ReactiveFormsModule, FormGroup, FormControl, FormArray, Validators } from '@angular/forms'` | `import { form, FormField, required, email, minLength, maxLength, pattern, min, max } from '@angular/forms/signals'` |
| `import { FormsModule } from '@angular/forms'` | `import { form, FormField } from '@angular/forms/signals'` |

#### Component imports array

| Before | After |
|--------|-------|
| `ReactiveFormsModule` | `FormField` |
| `FormsModule` | `FormField` |

#### Form model

| Before (Reactive) | Before (Template) | After (Signal) |
|--------------------|-------------------|----------------|
| `new FormGroup({ name: new FormControl('') })` | Class properties + `[(ngModel)]` | `signal({ name: '' })` + `form(this.model)` |
| `new FormControl('', Validators.required)` | N/A | Schema function: `required(schemaPath.name)` |
| `new FormArray([...])` | N/A | `signal({ items: [{ ... }] })` |

#### Template bindings

| Before | After |
|--------|-------|
| `[formGroup]="form"` | Remove entirely (no form-level directive needed) |
| `formControlName="name"` | `[formField]="myForm.name"` |
| `[formControl]="control"` | `[formField]="myForm.fieldName"` |
| `formGroupName="address"` | Access nested: `[formField]="myForm.address.street"` |
| `formArrayName="items"` | Access by index: `myForm.items[i].field` |
| `[(ngModel)]="property"` | `[formField]="myForm.property"` |

#### Validation display

| Before | After |
|--------|-------|
| `form.get('name')?.hasError('required')` | `myForm.name().invalid()` |
| `form.get('name')?.touched` | `myForm.name().touched()` |
| `form.get('name')?.dirty` | `myForm.name().dirty()` |
| `form.get('name')?.errors` | `myForm.name().errors()` |
| `form.get('name')?.valid` | `myForm.name().valid()` |
| `form.invalid` | `myForm().invalid()` |
| `form.valid` | `myForm().valid()` |

#### Value access

| Before | After |
|--------|-------|
| `form.value` | `model()` (read the signal directly) |
| `form.getRawValue()` | `model()` |
| `form.patchValue({ name: 'x' })` | `model.update(v => ({ ...v, name: 'x' }))` |
| `form.setValue(...)` | `model.set(...)` |
| `form.reset()` | `model.set(initialValue)` |
| `form.valueChanges` | Use `effect()` or `computed()` on the signal |
| `form.statusChanges` | `myForm().valid()` / `myForm().invalid()` in `computed()` |

#### Validation rules

| Before (Reactive) | After (Signal) |
|--------------------|----------------|
| `Validators.required` | `required(schemaPath.field, { message: '...' })` |
| `Validators.email` | `email(schemaPath.field, { message: '...' })` |
| `Validators.minLength(n)` | `minLength(schemaPath.field, n, { message: '...' })` |
| `Validators.maxLength(n)` | `maxLength(schemaPath.field, n, { message: '...' })` |
| `Validators.min(n)` | `min(schemaPath.field, n, { message: '...' })` |
| `Validators.max(n)` | `max(schemaPath.field, n, { message: '...' })` |
| `Validators.pattern(regex)` | `pattern(schemaPath.field, regex, { message: '...' })` |
| Custom sync validator | Use `validate()` rule (see below) |
| Custom async validator | Use `validateAsync()` rule (see below) |

#### Disabled state

| Before | After |
|--------|-------|
| `control.disable()` / `control.enable()` | `disabled(schemaPath.field, () => someSignal())` (declarative, signal-driven) |
| `new FormControl({ value: '', disabled: true })` | `disabled(schemaPath.field, () => true)` in schema function |

### 3. Handle edge cases

#### Third-party UI components (PrimeNG, Material, etc.)

Signal Forms `[formField]` supports three control types:
1. **Native HTML elements** (`<input>`, `<select>`, `<textarea>`) — first-class support
2. **Signal Forms custom controls** (implements `FormValueControl` or `FormCheckboxControl`)
3. **ControlValueAccessor components** — backward-compatible, works with most third-party libraries

**Strategy**: Try `[formField]` directly on the third-party component first. If it causes type errors or doesn't work (some PrimeNG components have conflicting `formField` properties), fall back to native HTML equivalents:
- `p-select` / `p-dropdown` → `<select>` with `[formField]`
- `p-checkbox` → `<input type="checkbox">` with `[formField]`
- `p-inputText` → `<input>` with `[formField]`
- `p-textarea` → `<textarea>` with `[formField]`
- `p-inputNumber` → `<input type="number">` with `[formField]`
- `p-calendar` / `p-datePicker` → `<input type="date">` with `[formField]`

If the third-party component MUST be used (for complex functionality like multiselect, autocomplete, etc.), keep it with `ControlValueAccessor` compatibility — `[formField]` supports CVA as a fallback.

#### FormArray / Dynamic arrays

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
new FormGroup({ password: new FormControl(''), confirm: new FormControl('') }, {
  validators: passwordMatchValidator
});

// After: use validate() on one field referencing another
form(this.model, (schemaPath) => {
  validate(schemaPath.confirm, () => {
    const m = this.model();
    return m.password === m.confirm ? null : { kind: 'passwordMismatch', message: 'Passwords must match' };
  });
});
```

#### Incremental migration with compatForm

If the form is very complex or uses many custom validators that are hard to port, use `compatForm` from `@angular/forms/signals/compat` to wrap existing `FormControl`/`FormGroup` instances inside a signal form. This allows gradual migration:

```typescript
import { compatForm } from '@angular/forms/signals/compat';

// Keep existing FormControl with complex validators
const emailControl = new FormControl('', [Validators.required, customAsyncValidator()]);

// Wrap in signal form
model = signal({ email: emailControl, name: '' });
myForm = compatForm(this.model);
```

#### Conditional validation with applyWhen

```typescript
form(this.model, (schemaPath) => {
  applyWhen(schemaPath.companyName, () => this.isBusinessAccount(), (p) => {
    required(p, { message: 'Company name is required for business accounts' });
  });
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

- Signal forms are experimental in Angular 21 — API may evolve
- `[formField]` on native elements is the most reliable binding method
- Never mix `ReactiveFormsModule`/`FormsModule` directives with `FormField` on the same control
- The `form()` function returns a field tree, not a signal — call `myForm()` to get the root state signal, and `myForm.field()` to get a specific field's state
- Array fields are accessed by index: `myForm.items[0].name` — the form tree automatically tracks array mutations
- Always add `novalidate` to `<form>` elements to prevent browser validation from conflicting 
