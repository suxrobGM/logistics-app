import { Component } from "@angular/core";
import {
  Badge,
  DetailSection,
  InfoItem,
  InfoRow,
  Stack,
  StatItem,
  Typography,
  UiButton,
} from "@logistics/shared/ui";

/**
 * The detail-page building blocks: ui-detail-section (titled card + header/actions slots),
 * ui-info-row (horizontal label/value with optional divider), ui-info-item (stacked label/value
 * with an empty-state), and ui-stat-item (caption + intent-tinted value).
 *
 * All four are rendered with real-ish content so a screenshot proves the header slot, the divider
 * grouping, the projected-vs-`value` paths, the `emptyText` fallback, and every stat intent.
 */
@Component({
  selector: "app-ui-lab-details",
  templateUrl: "./details-section.html",
  imports: [DetailSection, InfoRow, InfoItem, StatItem, Stack, Badge, Typography, UiButton],
})
export class UiLabDetailsSection {}
