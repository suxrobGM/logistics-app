import { Component, input, output } from "@angular/core";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";

@Component({
  selector: "ui-search-input",
  templateUrl: "./search-input.html",
  imports: [IconFieldModule, InputIconModule, InputTextModule],
  host: { class: "block" },
})
export class SearchInput {
  public readonly placeholder = input<string>("Search");
  public readonly class = input<string>("");
  public readonly searchChange = output<string>();

  protected handleInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchChange.emit(value);
  }
}
