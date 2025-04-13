export class DateRange {
  constructor(displayName: string, startDate: Date, endDate: Date) {
    this.displayName = displayName;
    this.startDate = startDate;
    this.endDate = endDate;
  }

  public readonly displayName;
  public readonly startDate;
  public readonly endDate;

  toString(): string {
    return this.displayName;
  }
}
