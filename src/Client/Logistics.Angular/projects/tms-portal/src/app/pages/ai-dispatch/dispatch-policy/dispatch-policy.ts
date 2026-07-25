import { DatePipe } from "@angular/common";
import { Component, computed, inject, linkedSignal, signal, type OnInit } from "@angular/core";
import { form, FormField, FormRoot, maxLength, submit } from "@angular/forms/signals";
import { getApiErrorMessage, PageHeader } from "@logistics/shared";
import {
  Api,
  deleteAiDispatchPolicy,
  getAiDispatchPolicy,
  regenerateAiDispatchPolicy,
  updateAiDispatchPolicy,
  type AiDispatchPolicyDto,
} from "@logistics/shared/api";
import {
  Card,
  Icon,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiFormField,
  UiTextareaField,
  UiToggleField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { MarkdownPipe } from "../utils/markdown";

/** Mirrors `DispatchPolicyText.MaxContentChars` on the server. */
const MAX_DIRECTIVES_LENGTH = 4000;

@Component({
  selector: "app-dispatch-policy",
  templateUrl: "./dispatch-policy.html",
  imports: [
    Card,
    DatePipe,
    FormField,
    FormRoot,
    Icon,
    MarkdownPipe,
    PageHeader,
    Spinner,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiFormField,
    UiTextareaField,
    UiToggleField,
    ValidatedForm,
  ],
})
export class DispatchPolicyPage implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly isRegenerating = signal(false);
  protected readonly isDeleting = signal(false);
  protected readonly isSavingToggle = signal(false);
  protected readonly showPreview = signal(false);

  protected readonly maxLength = MAX_DIRECTIVES_LENGTH;

  /** Server state - everything editable derives from it. */
  protected readonly policy = signal<AiDispatchPolicyDto>(emptyPolicy());

  /**
   * Editable mirrors that reset whenever server state changes, so a failed write only has to leave
   * `policy` alone instead of rolling anything back.
   */
  protected readonly isEnabled = linkedSignal(() => this.policy().isEnabled ?? true);
  protected readonly model = linkedSignal(() => ({
    manualContent: this.policy().manualContent ?? "",
  }));

  protected readonly directivesForm = form(
    this.model,
    (path) => {
      maxLength(path.manualContent, MAX_DIRECTIVES_LENGTH, {
        message: `Directives cannot exceed ${MAX_DIRECTIVES_LENGTH} characters.`,
      });
    },
    {
      submission: {
        action: async () => {
          await this.persist("Directives saved");
          return undefined;
        },
      },
    },
  );

  protected readonly hasLearned = computed(
    () => (this.policy().generatedContent ?? "").trim().length > 0,
  );

  protected readonly isEmpty = computed(
    () => !this.hasLearned() && (this.policy().manualContent ?? "").trim().length === 0,
  );

  ngOnInit(): void {
    this.load();
  }

  protected async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      this.policy.set(await this.api.invoke(getAiDispatchPolicy));
    } catch {
      this.toastService.showError("Failed to load the dispatch policy");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected submitDirectives(): void {
    submit(this.directivesForm);
  }

  /** The toggle pauses learning too, so it saves on change rather than on form submit. */
  protected async onEnabledChange(): Promise<void> {
    this.isSavingToggle.set(true);
    try {
      await this.persist(
        this.isEnabled() ? "Learned preferences re-enabled" : "Learned preferences paused",
      );
    } finally {
      this.isSavingToggle.set(false);
    }
  }

  /**
   * One writer for the whole document - the toggle and the directives share a PUT, so they cannot
   * drift apart. Updates the local copy instead of re-fetching: the server returns 204 and we
   * already know both values it changed.
   */
  private async persist(successMessage: string): Promise<void> {
    const manualContent = this.model().manualContent;
    const isEnabled = this.isEnabled();

    try {
      await this.api.invoke(updateAiDispatchPolicy, { body: { manualContent, isEnabled } });
      this.policy.update((p) => ({ ...p, manualContent, isEnabled }));
      this.toastService.showSuccess(successMessage);
    } catch {
      // `policy` is unchanged, so the linked signals snap the controls back to server state.
      this.policy.update((p) => ({ ...p }));
      this.toastService.showError("Failed to update the policy");
    }
  }

  protected async regenerate(): Promise<void> {
    this.isRegenerating.set(true);
    try {
      this.policy.set(await this.api.invoke(regenerateAiDispatchPolicy));
      this.toastService.showSuccess("Policy regenerated from recent decisions");
    } catch (error: unknown) {
      // A skip ("not enough reviewed decisions yet") arrives as a 400 - show that reason, it tells
      // the dispatcher what to do next.
      this.toastService.showError(getApiErrorMessage(error, "Failed to regenerate the policy"));
    } finally {
      this.isRegenerating.set(false);
    }
  }

  protected confirmDelete(): void {
    this.toastService.confirm({
      message:
        "This erases everything the agent has learned, plus your directives.\n\n" +
        "The nightly pass will start learning again from your decision history unless you also " +
        "switch off “Apply learned preferences”.",
      header: "Delete Dispatch Policy",
      icon: "warning",
      severity: "danger",
      accept: () => this.deletePolicy(),
    });
  }

  private async deletePolicy(): Promise<void> {
    this.isDeleting.set(true);
    try {
      await this.api.invoke(deleteAiDispatchPolicy);
      this.policy.set(emptyPolicy());
      this.toastService.showSuccess("Dispatch policy deleted");
    } catch {
      this.toastService.showError("Failed to delete the policy");
    } finally {
      this.isDeleting.set(false);
    }
  }
}

/** Mirrors `AiDispatchPolicyDto.Empty` on the server: a tenant with no policy row. */
function emptyPolicy(): AiDispatchPolicyDto {
  return { isEnabled: true };
}
