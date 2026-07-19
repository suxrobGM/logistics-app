import { Component } from "@angular/core";
import { Card, Container, Typography } from "@logistics/shared/ui";
import { PageHeader } from "@/shared/components";
import { ApiKeysTable, McpIntegrationGuide } from "../components";

@Component({
  selector: "app-api-keys-settings",
  templateUrl: "./api-keys-settings.html",
  imports: [ApiKeysTable, Card, Container, McpIntegrationGuide, PageHeader, Typography],
})
export class ApiKeysSettings {}
