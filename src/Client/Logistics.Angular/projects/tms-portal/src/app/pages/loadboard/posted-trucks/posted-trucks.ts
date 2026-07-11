import { Component, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { type PostTruckToLoadBoardCommand } from "@logistics/shared/api";
import { Alert, Spinner, Stack, Typography, UiButton } from "@logistics/shared/ui";
import { DashboardCard, EmptyState, ErrorState, PageHeader } from "@/shared/components";
import { PostedTrucksTable, PostTruckDialog } from "../_components";
import { LoadBoardStore } from "../store";

@Component({
  selector: "app-posted-trucks",
  templateUrl: "./posted-trucks.html",
  imports: [
    Alert,
    DashboardCard,
    EmptyState,
    ErrorState,
    PageHeader,
    PostedTrucksTable,
    PostTruckDialog,
    Spinner,
    Stack,
    Typography,
    UiButton,
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
