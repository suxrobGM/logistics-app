/**
 * Download a file from a Blob object.
 * @param blob The Blob object to download.
 * @param fileName The name to save the file as.
 */
export function downloadBlobFile(blob: Blob, fileName: string): void {
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  a.click();
  window.URL.revokeObjectURL(url);
}

/**
 * Format file size in bytes to a human-readable string (e.g., KB, MB, GB).
 * @param bytes The file size in bytes.
 * @returns A formatted string representing the file size.
 */
export function formatFileSize(bytes?: number): string {
  if (!bytes || bytes === 0) {
    return "0 Bytes";
  }
  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
}
