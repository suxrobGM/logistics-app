export enum DocumentOwnerType {
  Load = "load",
  Employee = "employee",
}

export enum DocumentStatus {
  Active = "active",
  Archived = "archived",
  Deleted = "deleted",
}

export enum DocumentType {
  BillOfLading = "bill_of_lading",
  ProofOfDelivery = "proof_of_delivery",
  Invoice = "invoice",
  Receipt = "receipt",
  Contract = "contract",
  InsuranceCertificate = "insurance_certificate",
  Photo = "photo",
  DriverLicense = "driver_license",
  VehicleRegistration = "vehicle_registration",
  IdentityDocument = "identity_document",
  Other = "other",
}
