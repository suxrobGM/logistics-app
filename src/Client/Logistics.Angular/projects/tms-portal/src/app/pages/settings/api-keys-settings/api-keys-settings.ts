import { Component } from "@angular/core";
import { Stack, Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { ApiKeysTable, McpIntegrationGuide } from "../_components";

@Component({
  selector: "app-api-keys-settings",
  templateUrl: "./api-keys-settings.html",
  imports: [CardModule, ApiKeysTable, McpIntegrationGuide, Stack, Typography],
})
export class ApiKeysSettings {}
