import { Component, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { type PostTruckToLoadBoardCommand } from "@logistics/shared/api";
import { Alert, Stack, Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { DashboardCard, EmptyState, ErrorState, PageHeader } from "@/shared/components";
import { PostedTrucksTable, PostTruckDialog } from "../_components";
import { LoadBoardStore } from "../store";

@Component({
  selector: "app-posted-trucks",
  templateUrl: "./posted-trucks.html",
  imports: [
    Alert,
    ButtonModule,
    DashboardCard,
    EmptyState,
    ErrorState,
    PageHeader,
    PostTruckDialog,
    PostedTrucksTable,
    ProgressSpinnerModule,
    Stack,
    Typography,
  ],
})
export class PostedTrucksComponent implements OnInit {
  private readonly router = inject(Router);
  protected readonly store = inject(LoadBoardStore);

  protected readonly showPostDialog = signal(false);
  protected readonly posting = signal(false);

  ngOnInit(): void {
    void this.store.loadPostedTrucks();
  }

  protected configureProviders(): void {
    this.router.navigateByUrl("/loadboard/providers");
  }

  protected async onPost(body: PostTruckToLoadBoardCommand): Promise<void> {
    this.posting.set(true);
    const ok = await this.store.postTruck(body);
    this.posting.set(false);
    if (ok) this.showPostDialog.set(false);
  }
}
