import type { BadgeSeverity } from "../badge/badge";

export type StatusKind = "load" | "truck" | "container" | "subscription" | "invoice" | "employee";

type SeverityMap = Record<string, BadgeSeverity>;

const loadSeverity: SeverityMap = {
  draft: "secondary",
  dispatched: "info",
  pickedup: "info",
  intransit: "info",
  delivered: "success",
  completed: "success",
  cancelled: "danger",
  exception: "warn",
};

const truckSeverity: SeverityMap = {
  available: "success",
  onroute: "info",
  inservice: "success",
  maintenance: "warn",
  outofservice: "danger",
  retired: "secondary",
};

const containerSeverity: SeverityMap = {
  available: "success",
  atport: "info",
  intransit: "info",
  delivered: "success",
  returned: "secondary",
  cancelled: "danger",
};

const subscriptionSeverity: SeverityMap = {
  active: "success",
  trialing: "info",
  pastdue: "warn",
  unpaid: "warn",
  canceled: "danger",
  cancelled: "danger",
  incomplete: "secondary",
};

const invoiceSeverity: SeverityMap = {
  draft: "secondary",
  sent: "info",
  pending: "info",
  paid: "success",
  overdue: "warn",
  cancelled: "danger",
};

const employeeSeverity: SeverityMap = {
  active: "success",
  inactive: "secondary",
  suspended: "warn",
  terminated: "danger",
};

const severityMaps: Record<StatusKind, SeverityMap> = {
  load: loadSeverity,
  truck: truckSeverity,
  container: containerSeverity,
  subscription: subscriptionSeverity,
  invoice: invoiceSeverity,
  employee: employeeSeverity,
};

/**
 * Maps a status string for a given entity kind to the matching badge severity.
 * Falls back to `info` for unknown statuses, `secondary` for null/empty.
 */
export function resolveStatusSeverity(
  kind: StatusKind,
  status: string | null | undefined,
): BadgeSeverity {
  if (!status) return "secondary";
  const key = status.replace(/\s+/g, "").toLowerCase();
  return severityMaps[kind][key] ?? "info";
}
