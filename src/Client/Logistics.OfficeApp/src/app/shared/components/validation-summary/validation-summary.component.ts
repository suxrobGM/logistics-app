import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {AsyncPipe, CommonModule} from '@angular/common';
import {FormGroup} from '@angular/forms';
import {BehaviorSubject, Subject, takeUntil} from 'rxjs';


@Component({
  selector: 'app-validation-summary',
  standalone: true,
  templateUrl: './validation-summary.component.html',
  styleUrls: ['./validation-summary.component.scss'],
  imports: [
    CommonModule,
    AsyncPipe
  ],
})
export class ValidationSummaryComponent implements OnInit {
  private formErrorsSubject = new BehaviorSubject<string[]>([]);
  public formErrors$ = this.formErrorsSubject.asObservable();

  @Input({required: true}) form!: FormGroup;
  

  ngOnInit(): void {
    this.form.valueChanges
    .subscribe(() => {
      const errors = this.calculateErrors();
      this.formErrorsSubject.next(errors);
    });
  }

  private calculateErrors(): string[] {
    return Object.keys(this.form.controls)
    .filter(controlName => {
      const control = this.form.controls[controlName];
      return control.errors && (control.touched || control.dirty);
    })
    .flatMap(controlName => {
      const control = this.form.controls[controlName];
      return Object.keys(control.errors ?? {})
        .filter(errorKey => Object.prototype.hasOwnProperty.call(control.errors, errorKey))
        .map(errorKey => this.getErrorDescription(controlName, errorKey))
        .filter((message): message is string => Boolean(message));
    });
  }

  private getErrorDescription(controlName: string, errorKey: string): string | null {
    const control = this.form.controls[controlName];
    
    if (!control.errors) {
      return null;
    }

    const defaultMessages: Record<string, string> = {
      required: `${controlName} is required.`,
      minlength: `${controlName} must be at least ${control.errors[errorKey]?.requiredLength} characters long.`,
      maxlength: `${controlName} can't be more than ${control.errors[errorKey]?.requiredLength} characters long.`,
      pattern: `${controlName} has an invalid format.`,
      min: `${controlName} must be greater than or equal to ${control.errors[errorKey]?.min}.`,
      max: `${controlName} must be less than or equal to ${control.errors[errorKey]?.max}.`
    };

    return defaultMessages[errorKey];
  }
}
