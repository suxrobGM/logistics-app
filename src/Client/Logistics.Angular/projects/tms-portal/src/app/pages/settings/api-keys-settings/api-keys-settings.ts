import { Component } from "@angular/core";
import { CardModule } from "primeng/card";
import { ApiKeysTable, McpIntegrationGuide } from "../_components";

@Component({
  selector: "app-api-keys-settings",
  templateUrl: "./api-keys-settings.html",
  imports: [CardModule, ApiKeysTable, McpIntegrationGuide],
})
export class ApiKeysSettings {}
