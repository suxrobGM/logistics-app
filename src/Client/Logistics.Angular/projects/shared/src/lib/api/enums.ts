/**
 * Enum-like const objects for backwards compatibility.
 * The generated code uses string union types, but consuming code expects enum-like objects.
 * These shadow the type-only exports from generated/models with both type + value.
 */
import type { SelectOption } from "../models/select-option";
import type {
  CustomerStatus,
  DocumentType,
  EmployeeStatus,
  InvoiceLineItemType,
  InvoiceStatus,
  LoadExceptionType,
  LoadStatus,
  LoadType,
  PaymentMethodType,
  PaymentStatus,
  SalaryType,
  SubscriptionStatus,
  TimeEntryType,
  TripStatus,
  TruckStatus,
  TruckType,
  UsBankAccountHolderType,
  UsBankAccountType,
} from "./generated";

// Options arrays for select dropdowns
export const paymentMethodTypeOptions: SelectOption<PaymentMethodType>[] = [
  { label: "Card", value: "card" },
  { label: "US Bank Account", value: "us_bank_account" },
  { label: "International Bank Account", value: "international_bank_account" },
  { label: "Cash", value: "cash" },
  { label: "Check", value: "check" },
];

export const usBankAccountHolderTypeOptions: SelectOption<UsBankAccountHolderType>[] = [
  { label: "Individual", value: "individual" },
  { label: "Business", value: "business" },
];

export const usBankAccountTypeOptions: SelectOption<UsBankAccountType>[] = [
  { label: "Checking", value: "checking" },
  { label: "Savings", value: "savings" },
];

export const salaryTypeOptions: SelectOption<SalaryType>[] = [
  { label: "None", value: "none" },
  { label: "Monthly", value: "monthly" },
  { label: "Weekly", value: "weekly" },
  { label: "Share of Gross", value: "share_of_gross" },
  { label: "Rate per Distance", value: "rate_per_distance" },
  { label: "Hourly", value: "hourly" },
];

export const employeeStatusOptions: SelectOption<EmployeeStatus>[] = [
  { label: "Active", value: "active" },
  { label: "On Leave", value: "on_leave" },
  { label: "Suspended", value: "suspended" },
  { label: "Terminated", value: "terminated" },
];

export const customerStatusOptions: SelectOption<CustomerStatus>[] = [
  { label: "Active", value: "active" },
  { label: "Inactive", value: "inactive" },
  { label: "Prospect", value: "prospect" },
];

export const documentTypeOptions: SelectOption<DocumentType>[] = [
  { label: "Bill of Lading", value: "bill_of_lading" },
  { label: "Proof of Delivery", value: "proof_of_delivery" },
  { label: "Invoice", value: "invoice" },
  { label: "Receipt", value: "receipt" },
  { label: "Contract", value: "contract" },
  { label: "Insurance Certificate", value: "insurance_certificate" },
  { label: "Photo", value: "photo" },
  { label: "Driver License", value: "driver_license" },
  { label: "Vehicle Registration", value: "vehicle_registration" },
  { label: "Identity Document", value: "identity_document" },
  { label: "Other", value: "other" },
];

export const loadTypeOptions: SelectOption<LoadType>[] = [
  { label: "General Freight", value: "general_freight" },
  { label: "Refrigerated Goods", value: "refrigerated_goods" },
  { label: "Hazardous Materials", value: "hazardous_materials" },
  { label: "Oversize/Heavy", value: "oversize_heavy" },
  { label: "Liquid", value: "liquid" },
  { label: "Bulk", value: "bulk" },
  { label: "Vehicle", value: "vehicle" },
  { label: "Livestock", value: "livestock" },
];

export const truckTypeOptions: SelectOption<TruckType>[] = [
  { label: "Flatbed", value: "flatbed" },
  { label: "Freight Truck", value: "freight_truck" },
  { label: "Reefer", value: "reefer" },
  { label: "Tanker", value: "tanker" },
  { label: "Box Truck", value: "box_truck" },
  { label: "Dump Truck", value: "dump_truck" },
  { label: "Tow Truck", value: "tow_truck" },
  { label: "Car Hauler", value: "car_hauler" },
];

export const truckStatusOptions: SelectOption<TruckStatus>[] = [
  { label: "Available", value: "available" },
  { label: "En Route", value: "en_route" },
  { label: "Loading", value: "loading" },
  { label: "Unloading", value: "unloading" },
  { label: "Maintenance", value: "maintenance" },
  { label: "Out of Service", value: "out_of_service" },
  { label: "Offline", value: "offline" },
];

export const loadStatusOptions: SelectOption<LoadStatus>[] = [
  { label: "Draft", value: "draft" },
  { label: "Dispatched", value: "dispatched" },
  { label: "Picked Up", value: "picked_up" },
  { label: "Delivered", value: "delivered" },
  { label: "Cancelled", value: "cancelled" },
];

export const tripStatusOptions: SelectOption<TripStatus>[] = [
  { label: "Draft", value: "draft" },
  { label: "Dispatched", value: "dispatched" },
  { label: "In Transit", value: "in_transit" },
  { label: "Completed", value: "completed" },
  { label: "Cancelled", value: "cancelled" },
];

export const invoiceStatusOptions: SelectOption<InvoiceStatus>[] = [
  { label: "Draft", value: "draft" },
  { label: "Pending Approval", value: "pending_approval" },
  { label: "Approved", value: "approved" },
  { label: "Rejected", value: "rejected" },
  { label: "Issued", value: "issued" },
  { label: "Partially Paid", value: "partially_paid" },
  { label: "Paid", value: "paid" },
  { label: "Cancelled", value: "cancelled" },
];

export const paymentStatusOptions: SelectOption<PaymentStatus>[] = [
  { label: "Pending", value: "pending" },
  { label: "Paid", value: "paid" },
  { label: "Failed", value: "failed" },
  { label: "Cancelled", value: "cancelled" },
  { label: "Refunded", value: "refunded" },
  { label: "Partially Refunded", value: "partially_refunded" },
];

export const subscriptionStatusOptions: SelectOption<SubscriptionStatus>[] = [
  { label: "Active", value: "active" },
  { label: "Incomplete", value: "incomplete" },
  { label: "Incomplete Expired", value: "incomplete_expired" },
  { label: "Trialing", value: "trialing" },
  { label: "Past Due", value: "past_due" },
  { label: "Cancelled", value: "cancelled" },
  { label: "Unpaid", value: "unpaid" },
  { label: "Paused", value: "paused" },
];

export const timeEntryTypeOptions: SelectOption<TimeEntryType>[] = [
  { label: "Regular", value: "regular" },
  { label: "Overtime", value: "overtime" },
  { label: "Double Time", value: "double_time" },
  { label: "Paid Time Off", value: "paid_time_off" },
  { label: "Holiday", value: "holiday" },
];

export const invoiceLineItemTypeOptions: SelectOption<InvoiceLineItemType>[] = [
  { label: "Base Rate", value: "base_rate" },
  { label: "Fuel Surcharge", value: "fuel_surcharge" },
  { label: "Detention", value: "detention" },
  { label: "Layover", value: "layover" },
  { label: "Lumper", value: "lumper" },
  { label: "Accessorial", value: "accessorial" },
  { label: "Discount", value: "discount" },
  { label: "Tax", value: "tax" },
  { label: "Other", value: "other" },
];

export const payrollLineItemTypeOptions: SelectOption<InvoiceLineItemType>[] = [
  { label: "Base Pay", value: "base_pay" },
  { label: "Bonus", value: "bonus" },
  { label: "Deduction", value: "deduction" },
  { label: "Reimbursement", value: "reimbursement" },
  { label: "Adjustment", value: "adjustment" },
];

export const loadExceptionTypeOptions: SelectOption<LoadExceptionType>[] = [
  { label: "Delay", value: "delay" },
  { label: "Accident", value: "accident" },
  { label: "Weather Delay", value: "weather_delay" },
  { label: "Mechanical Failure", value: "mechanical_failure" },
  { label: "Route Change", value: "route_change" },
  { label: "Customer Request", value: "customer_request" },
  { label: "Other", value: "other" },
];
