/**
 * Convert hex color to rgba string with specified alpha transparency.
 * @param hex Color in hex format (e.g., "#ff5733")
 * @param alpha Alpha transparency (0 to 1)
 * @returns Color in rgba format (e.g., "rgba(255, 87, 51, 0.5)")
 */
export function hexToRgba(hex: string, alpha: number): string {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);

  if (!result) {
    return `rgba(0, 0, 0, ${alpha})`;
  }

  const r = parseInt(result[1], 16);
  const g = parseInt(result[2], 16);
  const b = parseInt(result[3], 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

/**
 * Adjusts the brightness of a hex color by a given percent.
 * @param hex Color in hex format (e.g., #RRGGBB)
 * @param percent Positive to lighten, negative to darken
 * @returns Adjusted color in hex format
 */
export function adjustColorBrightness(hex: string, percent: number): string {
  const num = parseInt(hex.replace("#", ""), 16);
  const r = Math.min(255, Math.max(0, (num >> 16) + percent));
  const g = Math.min(255, Math.max(0, ((num >> 8) & 0x00ff) + percent));
  const b = Math.min(255, Math.max(0, (num & 0x0000ff) + percent));
  return `#${((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1)}`;
}
