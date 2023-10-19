export abstract class NumberUtils {
  /**
   * Converts a number to the percentage value [0, 100]
   */
  static toPercent(value: number): number {
    return value * 100.0;
  }

  /**
   * Converts a percentage value to the ratio [0, 1]
   * @param percentage
   * @returns
   */
  static toRatio(percentage: number): number {
    return percentage / 100.0;
  }
}
