import { Component, DestroyRef, type OnInit, inject, input, output } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { Subject, debounceTime, distinctUntilChanged } from "rxjs";

@Component({
  selector: "ui-search-input",
  templateUrl: "./search-input.html",
  imports: [IconFieldModule, InputIconModule, InputTextModule],
  host: { class: "block" },
})
export class SearchInput implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchSubject = new Subject<string>();

  public readonly placeholder = input<string>("Search");
  public readonly class = input<string>("");
  public readonly debounce = input<number>(300);
  public readonly searchChange = output<string>();

  ngOnInit(): void {
    this.searchSubject
      .pipe(
        debounceTime(this.debounce()),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((value) => this.searchChange.emit(value));
  }

  protected handleInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject.next(value);
  }
}
