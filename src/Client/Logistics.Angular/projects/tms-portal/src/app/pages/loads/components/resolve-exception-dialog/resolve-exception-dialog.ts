import { Component, computed, inject, input, model, output, signal } from "@angular/core";
import { UiFormField } from "@logistics/shared";
import { Api, resolveLoadException, type LoadExceptionDto } from "@logistics/shared/api";
import { UiButton, UiDialog, UiTextareaField } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { ExceptionTypeTag } from "@/shared/components/tags";

@Component({
  selector: "app-resolve-exception-dialog",
  templateUrl: "./resolve-exception-dialog.html",
  imports: [ExceptionTypeTag, UiButton, UiDialog, UiFormField, UiTextareaField],
})
export class ResolveExceptionDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly loadId = input.required<string>();
  public readonly exception = input<LoadExceptionDto | null>(null);
  public readonly visible = model<boolean>(false);
  public readonly resolved = output<void>();

  protected readonly resolution = signal<string>("");
  protected readonly isSubmitting = signal(false);

  protected readonly exceptionType = computed(() => this.exception()?.type);

  async submit(): Promise<void> {
    const exc = this.exception();
    const resolutionText = this.resolution().trim();

    if (!exc) {
      return;
    }

    if (!resolutionText) {
      this.toastService.showWarning("Please provide resolution notes");
      return;
    }

    this.isSubmitting.set(true);
    try {
      await this.api.invoke(resolveLoadException, {
        id: this.loadId(),
        exceptionId: exc.id!,
        body: { resolution: resolutionText },
      });
      this.toastService.showSuccess("Exception resolved successfully");
      this.resolved.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to resolve exception");
    } finally {
      this.isSubmitting.set(false);
    }
  }

  close(): void {
    this.resolution.set("");
    this.visible.set(false);
  }
}
