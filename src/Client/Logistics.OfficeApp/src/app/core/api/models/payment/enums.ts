import {SelectOption} from "@/core/types";

export enum PaymentMethodType {
  Card = 0,
  UsBankAccount = 1,
  InternationalBankAccount = 2,
}

export enum UsBankAccountHolderType {
  Individual = 0,
  Business = 1,
}

export enum UsBankAccountType {
  Checking = 0,
  Savings = 1,
}

export const pymentMethodTypeOptions: SelectOption<PaymentMethodType>[] = [
  {label: "Credit/Debit Card", value: PaymentMethodType.Card},
  {label: "US Bank Account", value: PaymentMethodType.UsBankAccount},
  //{label: "International Bank", value: PaymentMethodType.InternationalBankAccount},
] as const;

export const usBankAccountHolderTypeOptions: SelectOption<UsBankAccountHolderType>[] = [
  {label: "Individual", value: UsBankAccountHolderType.Individual},
  {label: "Business", value: UsBankAccountHolderType.Business},
] as const;

export const usBankAccountTypeOptions: SelectOption<UsBankAccountType>[] = [
  {label: "Checking", value: UsBankAccountType.Checking},
  {label: "Savings", value: UsBankAccountType.Savings},
] as const;
