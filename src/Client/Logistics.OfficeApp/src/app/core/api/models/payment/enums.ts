import {SelectOption} from "@/core/types";

export enum PaymentMethodType {
  Card = "card",
  UsBankAccount = "us_bank_account",
  InternationalBankAccount = "international_bank_account",
}

export enum UsBankAccountHolderType {
  Individual = "individual",
  Business = "business",
}

export enum UsBankAccountType {
  Checking = "checking",
  Savings = "savings",
}

export enum PaymentFor {
  EmployeePayroll = "employee_payroll",
  Subscription = "subscription",
  Invoice = "invoice",
  Other = "other",
}

export enum PaymentStatus {
  Pending = "pending",
  Paid = "paid",
  Failed = "failed",
  Cancelled = "cancelled",
}

export enum PaymentMethodVerificationStatus {
  Unverified = "unverified",
  Pending = "pending",
  Failed = "failed",
  Verified = "verified",
}

export const paymentStatusOptions: SelectOption<PaymentStatus>[] = [
  {label: "Pending", value: PaymentStatus.Pending},
  {label: "Paid", value: PaymentStatus.Paid},
  {label: "Failed", value: PaymentStatus.Failed},
  {label: "Cancelled", value: PaymentStatus.Cancelled},
];

export const paymentForOptions: SelectOption<PaymentFor>[] = [
  {label: "Employee Payroll", value: PaymentFor.EmployeePayroll},
  {label: "Subscription", value: PaymentFor.Subscription},
  {label: "Invoice", value: PaymentFor.Invoice},
  {label: "Other", value: PaymentFor.Other},
] as const;

export const paymentMethodTypeOptions: SelectOption<PaymentMethodType>[] = [
  {label: "Credit/Debit Card", value: PaymentMethodType.Card},
  {label: "US Bank Account", value: PaymentMethodType.UsBankAccount},
  {label: "International Bank", value: PaymentMethodType.InternationalBankAccount},
] as const;

export const usBankAccountHolderTypeOptions: SelectOption<UsBankAccountHolderType>[] = [
  {label: "Individual", value: UsBankAccountHolderType.Individual},
  {label: "Business", value: UsBankAccountHolderType.Business},
] as const;

export const usBankAccountTypeOptions: SelectOption<UsBankAccountType>[] = [
  {label: "Checking", value: UsBankAccountType.Checking},
  {label: "Savings", value: UsBankAccountType.Savings},
] as const;
