export abstract class NumberUtils {
  static toPercentage(value: number): number {
    return value * 100.0;
  }

  static toRatio(percentage: number) {
    return percentage / 100.0;
  }
}
