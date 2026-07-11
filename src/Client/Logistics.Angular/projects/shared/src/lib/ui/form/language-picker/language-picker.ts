import { Component, computed, inject, input } from "@angular/core";
import { I18nService } from "../../../services/i18n.service";
import { UiSelectField } from "../select-field/select-field";

export interface LanguageOption {
  code: string;
  label: string;
}

@Component({
  selector: "ui-language-picker",
  templateUrl: "./language-picker.html",
  imports: [UiSelectField],
})
export class LanguagePicker {
  private readonly i18n = inject(I18nService);

  public readonly languages = input<LanguageOption[]>([{ code: "en", label: "English" }]);

  protected readonly value = computed(() => this.i18n.currentLanguage());

  protected onChange(code: string): void {
    this.i18n.setLanguage(code);
  }
}
