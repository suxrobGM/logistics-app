import { Component, inject } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { MessageModule } from "primeng/message";
import { TabsModule } from "primeng/tabs";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { environment } from "@/env";

@Component({
  selector: "app-mcp-integration-guide",
  templateUrl: "./mcp-integration-guide.html",
  imports: [ButtonModule, MessageModule, TabsModule, TooltipModule],
})
export class McpIntegrationGuide {
  private readonly toastService = inject(ToastService);

  protected readonly mcpEndpoint = `${environment.apiUrl}/mcp`;

  protected readonly claudeDesktopSnippet = JSON.stringify(
    {
      mcpServers: {
        logisticsx: {
          url: this.mcpEndpoint,
          headers: { Authorization: "Bearer <your-api-key>" },
        },
      },
    },
    null,
    2,
  );

  protected readonly claudeCodeSnippet = `claude mcp add logisticsx \\
  --transport http \\
  --url ${this.mcpEndpoint} \\
  --header "Authorization: Bearer <your-api-key>"`;

  protected readonly cursorSnippet = JSON.stringify(
    {
      mcpServers: {
        logisticsx: {
          url: this.mcpEndpoint,
          headers: { Authorization: "Bearer <your-api-key>" },
        },
      },
    },
    null,
    2,
  );

  protected readonly copilotSnippet = JSON.stringify(
    {
      servers: {
        logisticsx: {
          type: "http",
          url: this.mcpEndpoint,
          headers: { Authorization: "Bearer <your-api-key>" },
        },
      },
    },
    null,
    2,
  );

  protected readonly apiSnippet = `curl ${environment.apiUrl}/loads \\
  -H "Authorization: Bearer <your-api-key>" \\
  -H "Content-Type: application/json"`;

  protected async copyToClipboard(text: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(text);
      this.toastService.showSuccess("Copied to clipboard");
    } catch {
      this.toastService.showError("Failed to copy to clipboard");
    }
  }
}
