import { Component } from "@angular/core";
import { Container, Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { PageHeader } from "@/shared/components";
import { ApiKeysTable, McpIntegrationGuide } from "../_components";

@Component({
  selector: "app-api-keys-settings",
  templateUrl: "./api-keys-settings.html",
  imports: [CardModule, ApiKeysTable, McpIntegrationGuide, PageHeader, Container, Typography],
})
export class ApiKeysSettings {}
