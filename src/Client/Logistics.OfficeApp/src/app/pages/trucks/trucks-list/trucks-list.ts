import { Component, inject } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import type { AddressDto } from "@/core/api/models";
import { DataContainer } from "@/shared/components";
import { AddressPipe } from "@/shared/pipes";
import { TrucksListStore } from "../store/trucks-list.store";

@Component({
  selector: "app-trucks-list",
  templateUrl: "./trucks-list.html",
  providers: [TrucksListStore, AddressPipe],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    InputTextModule,
    AddressPipe,
    IconFieldModule,
    InputIconModule,
    DataContainer,
  ],
})
export class TrucksListComponent {
  private readonly router = inject(Router);
  private readonly addressPipe = inject(AddressPipe);
  protected readonly store = inject(TrucksListStore);

  protected search(event: Event): void {
    const searchValue = (event.target as HTMLInputElement).value;
    this.store.setSearch(searchValue);
  }

  protected addTruck(): void {
    this.router.navigate(["/trucks/add"]);
  }

  protected formatAddress(address: AddressDto): string {
    return this.addressPipe.transform(address) || "No address provided";
  }
}
