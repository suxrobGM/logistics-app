/**
 * Re-export all models from generated code for backwards compatibility.
 * Import from '@/core/api' or '@/core/api/models' - both work.
 *
 * Enum types are exported both as types and as const objects.
 * This allows using them both in type positions (e.g., status: SalaryType)
 * and as values (e.g., SalaryType.None).
 */
export * from "../generated/models";

// Backwards-compatible type aliases for renamed types
export type { Address as AddressDto } from "../generated/models/address";
export type { GeoPoint as GeoPointDto } from "../generated/models/geo-point";

/**
 * Enum-like const objects for backwards compatibility.
 * The generated code uses string union types, but consuming code expects enum-like objects.
 * These shadow the type-only exports from generated/models with both type + value.
 */

// SubscriptionStatus
export type SubscriptionStatus = "active" | "incomplete" | "incomplete_expired" | "trialing" | "past_due" | "cancelled" | "unpaid" | "paused";
export const SubscriptionStatus = {
  Active: "active" as const,
  Incomplete: "incomplete" as const,
  IncompleteExpired: "incomplete_expired" as const,
  Trialing: "trialing" as const,
  PastDue: "past_due" as const,
  Cancelled: "cancelled" as const,
  Unpaid: "unpaid" as const,
  Paused: "paused" as const,
};

// LoadStatus
export type LoadStatus = "draft" | "dispatched" | "picked_up" | "delivered" | "cancelled";
export const LoadStatus = {
  Draft: "draft" as const,
  Dispatched: "dispatched" as const,
  PickedUp: "picked_up" as const,
  Delivered: "delivered" as const,
  Cancelled: "cancelled" as const,
};

// TripStatus
export type TripStatus = "draft" | "dispatched" | "in_transit" | "completed" | "cancelled";
export const TripStatus = {
  Draft: "draft" as const,
  Dispatched: "dispatched" as const,
  InTransit: "in_transit" as const,
  Completed: "completed" as const,
  Cancelled: "cancelled" as const,
};

// TripStopType
export type TripStopType = "pick_up" | "drop_off";
export const TripStopType = {
  PickUp: "pick_up" as const,
  DropOff: "drop_off" as const,
};

// TruckStatus
export type TruckStatus = "available" | "en_route" | "loading" | "unloading" | "maintenance" | "out_of_service" | "offline";
export const TruckStatus = {
  Available: "available" as const,
  EnRoute: "en_route" as const,
  Loading: "loading" as const,
  Unloading: "unloading" as const,
  Maintenance: "maintenance" as const,
  OutOfService: "out_of_service" as const,
  Offline: "offline" as const,
};

// InvoiceStatus
export type InvoiceStatus = "draft" | "issued" | "partially_paid" | "paid" | "cancelled";
export const InvoiceStatus = {
  Draft: "draft" as const,
  Issued: "issued" as const,
  PartiallyPaid: "partially_paid" as const,
  Paid: "paid" as const,
  Cancelled: "cancelled" as const,
};

// PaymentStatus
export type PaymentStatus = "pending" | "paid" | "failed" | "cancelled" | "refunded" | "partially_refunded";
export const PaymentStatus = {
  Pending: "pending" as const,
  Paid: "paid" as const,
  Failed: "failed" as const,
  Cancelled: "cancelled" as const,
  Refunded: "refunded" as const,
  PartiallyRefunded: "partially_refunded" as const,
};

// InvoiceType
export type InvoiceType = "load" | "subscription" | "payroll" | "other";
export const InvoiceType = {
  Load: "load" as const,
  Subscription: "subscription" as const,
  Payroll: "payroll" as const,
  Other: "other" as const,
};

// PaymentMethodType
export type PaymentMethodType = "card" | "us_bank_account" | "international_bank_account" | "cash" | "check";
export const PaymentMethodType = {
  Card: "card" as const,
  UsBankAccount: "us_bank_account" as const,
  InternationalBankAccount: "international_bank_account" as const,
  Cash: "cash" as const,
  Check: "check" as const,
};

// PaymentMethodVerificationStatus
export type PaymentMethodVerificationStatus = "unverified" | "pending" | "failed" | "verified";
export const PaymentMethodVerificationStatus = {
  Unverified: "unverified" as const,
  Pending: "pending" as const,
  Failed: "failed" as const,
  Verified: "verified" as const,
};

// UsBankAccountHolderType
export type UsBankAccountHolderType = "individual" | "business";
export const UsBankAccountHolderType = {
  Individual: "individual" as const,
  Business: "business" as const,
};

// UsBankAccountType
export type UsBankAccountType = "checking" | "savings";
export const UsBankAccountType = {
  Checking: "checking" as const,
  Savings: "savings" as const,
};

// SalaryType
export type SalaryType = "none" | "monthly" | "weekly" | "share_of_gross" | "rate_per_mile" | "rate_per_kilometer" | "hourly";
export const SalaryType = {
  None: "none" as const,
  Monthly: "monthly" as const,
  Weekly: "weekly" as const,
  ShareOfGross: "share_of_gross" as const,
  RatePerMile: "rate_per_mile" as const,
  RatePerKilometer: "rate_per_kilometer" as const,
  Hourly: "hourly" as const,
};

// DocumentType
export type DocumentType = "bill_of_lading" | "proof_of_delivery" | "invoice" | "receipt" | "contract" | "insurance_certificate" | "photo" | "driver_license" | "vehicle_registration" | "identity_document" | "other";
export const DocumentType = {
  BillOfLading: "bill_of_lading" as const,
  ProofOfDelivery: "proof_of_delivery" as const,
  Invoice: "invoice" as const,
  Receipt: "receipt" as const,
  Contract: "contract" as const,
  InsuranceCertificate: "insurance_certificate" as const,
  Photo: "photo" as const,
  DriverLicense: "driver_license" as const,
  VehicleRegistration: "vehicle_registration" as const,
  IdentityDocument: "identity_document" as const,
  Other: "other" as const,
};

// LoadType
export type LoadType = "general_freight" | "refrigerated_goods" | "hazardous_materials" | "oversize_heavy" | "liquid" | "bulk" | "vehicle" | "livestock";
export const LoadType = {
  GeneralFreight: "general_freight" as const,
  RefrigeratedGoods: "refrigerated_goods" as const,
  HazardousMaterials: "hazardous_materials" as const,
  OversizeHeavy: "oversize_heavy" as const,
  Liquid: "liquid" as const,
  Bulk: "bulk" as const,
  Vehicle: "vehicle" as const,
  Livestock: "livestock" as const,
};

// TruckType
export type TruckType = "flatbed" | "freight_truck" | "reefer" | "tanker" | "box_truck" | "dump_truck" | "tow_truck" | "car_hauler";
export const TruckType = {
  Flatbed: "flatbed" as const,
  FreightTruck: "freight_truck" as const,
  Reefer: "reefer" as const,
  Tanker: "tanker" as const,
  BoxTruck: "box_truck" as const,
  DumpTruck: "dump_truck" as const,
  TowTruck: "tow_truck" as const,
  CarHauler: "car_hauler" as const,
};

// DocumentOwnerType
export type DocumentOwnerType = "load" | "employee" | "truck" | "trip" | "tenant" | "customer" | "invoice";
export const DocumentOwnerType = {
  Load: "load" as const,
  Employee: "employee" as const,
  Truck: "truck" as const,
  Trip: "trip" as const,
  Tenant: "tenant" as const,
  Customer: "customer" as const,
  Invoice: "invoice" as const,
};

// DocumentStatus
export type DocumentStatus = "active" | "archived" | "deleted";
export const DocumentStatus = {
  Active: "active" as const,
  Archived: "archived" as const,
  Deleted: "deleted" as const,
};

// Options arrays for select dropdowns
export const paymentMethodTypeOptions = [
  { label: "Card", value: PaymentMethodType.Card },
  { label: "US Bank Account", value: PaymentMethodType.UsBankAccount },
  { label: "International Bank Account", value: PaymentMethodType.InternationalBankAccount },
  { label: "Cash", value: PaymentMethodType.Cash },
  { label: "Check", value: PaymentMethodType.Check },
];

export const usBankAccountHolderTypeOptions = [
  { label: "Individual", value: UsBankAccountHolderType.Individual },
  { label: "Business", value: UsBankAccountHolderType.Business },
];

export const usBankAccountTypeOptions = [
  { label: "Checking", value: UsBankAccountType.Checking },
  { label: "Savings", value: UsBankAccountType.Savings },
];

export const salaryTypeOptions = [
  { label: "None", value: SalaryType.None },
  { label: "Monthly", value: SalaryType.Monthly },
  { label: "Weekly", value: SalaryType.Weekly },
  { label: "Share of Gross", value: SalaryType.ShareOfGross },
  { label: "Rate per Mile", value: SalaryType.RatePerMile },
  { label: "Rate per Kilometer", value: SalaryType.RatePerKilometer },
  { label: "Hourly", value: SalaryType.Hourly },
];

export const documentTypeOptions = [
  { label: "Bill of Lading", value: DocumentType.BillOfLading },
  { label: "Proof of Delivery", value: DocumentType.ProofOfDelivery },
  { label: "Invoice", value: DocumentType.Invoice },
  { label: "Receipt", value: DocumentType.Receipt },
  { label: "Contract", value: DocumentType.Contract },
  { label: "Insurance Certificate", value: DocumentType.InsuranceCertificate },
  { label: "Photo", value: DocumentType.Photo },
  { label: "Driver License", value: DocumentType.DriverLicense },
  { label: "Vehicle Registration", value: DocumentType.VehicleRegistration },
  { label: "Identity Document", value: DocumentType.IdentityDocument },
  { label: "Other", value: DocumentType.Other },
];

export const loadTypeOptions = [
  { label: "General Freight", value: LoadType.GeneralFreight },
  { label: "Refrigerated Goods", value: LoadType.RefrigeratedGoods },
  { label: "Hazardous Materials", value: LoadType.HazardousMaterials },
  { label: "Oversize/Heavy", value: LoadType.OversizeHeavy },
  { label: "Liquid", value: LoadType.Liquid },
  { label: "Bulk", value: LoadType.Bulk },
  { label: "Vehicle", value: LoadType.Vehicle },
  { label: "Livestock", value: LoadType.Livestock },
];

export const truckTypeOptions = [
  { label: "Flatbed", value: TruckType.Flatbed },
  { label: "Freight Truck", value: TruckType.FreightTruck },
  { label: "Reefer", value: TruckType.Reefer },
  { label: "Tanker", value: TruckType.Tanker },
  { label: "Box Truck", value: TruckType.BoxTruck },
  { label: "Dump Truck", value: TruckType.DumpTruck },
  { label: "Tow Truck", value: TruckType.TowTruck },
  { label: "Car Hauler", value: TruckType.CarHauler },
];

export const truckStatusOptions = [
  { label: "Available", value: TruckStatus.Available },
  { label: "En Route", value: TruckStatus.EnRoute },
  { label: "Loading", value: TruckStatus.Loading },
  { label: "Unloading", value: TruckStatus.Unloading },
  { label: "Maintenance", value: TruckStatus.Maintenance },
  { label: "Out of Service", value: TruckStatus.OutOfService },
  { label: "Offline", value: TruckStatus.Offline },
];

export const loadStatusOptions = [
  { label: "Draft", value: LoadStatus.Draft },
  { label: "Dispatched", value: LoadStatus.Dispatched },
  { label: "Picked Up", value: LoadStatus.PickedUp },
  { label: "Delivered", value: LoadStatus.Delivered },
  { label: "Cancelled", value: LoadStatus.Cancelled },
];

export const tripStatusOptions = [
  { label: "Draft", value: TripStatus.Draft },
  { label: "Dispatched", value: TripStatus.Dispatched },
  { label: "In Transit", value: TripStatus.InTransit },
  { label: "Completed", value: TripStatus.Completed },
  { label: "Cancelled", value: TripStatus.Cancelled },
];

export const invoiceStatusOptions = [
  { label: "Draft", value: InvoiceStatus.Draft },
  { label: "Issued", value: InvoiceStatus.Issued },
  { label: "Partially Paid", value: InvoiceStatus.PartiallyPaid },
  { label: "Paid", value: InvoiceStatus.Paid },
  { label: "Cancelled", value: InvoiceStatus.Cancelled },
];

export const paymentStatusOptions = [
  { label: "Pending", value: PaymentStatus.Pending },
  { label: "Paid", value: PaymentStatus.Paid },
  { label: "Failed", value: PaymentStatus.Failed },
  { label: "Cancelled", value: PaymentStatus.Cancelled },
  { label: "Refunded", value: PaymentStatus.Refunded },
  { label: "Partially Refunded", value: PaymentStatus.PartiallyRefunded },
];

export const subscriptionStatusOptions = [
  { label: "Active", value: SubscriptionStatus.Active },
  { label: "Incomplete", value: SubscriptionStatus.Incomplete },
  { label: "Incomplete Expired", value: SubscriptionStatus.IncompleteExpired },
  { label: "Trialing", value: SubscriptionStatus.Trialing },
  { label: "Past Due", value: SubscriptionStatus.PastDue },
  { label: "Cancelled", value: SubscriptionStatus.Cancelled },
  { label: "Unpaid", value: SubscriptionStatus.Unpaid },
  { label: "Paused", value: SubscriptionStatus.Paused },
];

// Re-export common query types used by consumers
export interface SearchableQuery {
  search?: string;
  orderBy?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedIntervalQuery extends SearchableQuery {
  startDate?: Date | string;
  endDate?: Date | string;
}

/**
 * Generic Result type for backwards compatibility.
 * The generated code uses specific types like LoadDtoResult, CustomerDtoResult.
 */
export interface Result<T = void> {
  data?: T | null;
  success?: boolean;
  error?: string | null;
}

/**
 * Generic PagedResult type for backwards compatibility.
 * The generated code uses specific types like LoadDtoPagedResult, CustomerDtoPagedResult.
 */
export interface PagedResult<T> {
  data?: T[] | null;
  totalItems?: number;
  totalPages?: number;
  success?: boolean;
  error?: string | null;
}

// Truck geolocation type for map visualization (custom type, not generated)
import type { Address } from "../generated/models/address";
import type { GeoPoint } from "../generated/models/geo-point";

export interface TruckGeolocationDto {
  truckId?: string | null;
  truckNumber?: string | null;
  driversName?: string | null;
  currentLocation?: GeoPoint | null;
  currentAddress?: Address | null;
}
