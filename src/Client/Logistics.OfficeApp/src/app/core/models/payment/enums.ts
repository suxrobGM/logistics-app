export enum PaymentMethodType {
  Card = 0,
  UsBankAccount = 1,
  InternationalBankAccount = 2,
}

export enum CardFundingType {
  Debit = 0,
  Credit = 1,
  Prepaid = 2,
}

export enum UsBankAccountHolderType {
  Individual = 0,
  Business = 1,
}

export enum UsBankAccountType {
  Checking = 0,
  Savings = 1,
}

export enum CardBrand {
  Visa = "Visa",
  MasterCard = "Mastercard",
  AmericanExpress = "American Express",
  Discover = "Discover",
}
