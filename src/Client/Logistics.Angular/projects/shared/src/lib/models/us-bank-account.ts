import type { UsBankAccountHolderType, UsBankAccountType } from "@logistics/shared/api/models";

export interface UsBankAccount {
  accountNumber: string;
  accountHolderName: string;
  bankName: string;
  routingNumber: string;
  accountHolderType: UsBankAccountHolderType;
  accountType: UsBankAccountType;
}
