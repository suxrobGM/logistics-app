namespace Logistics.Application.Utilities;

/// <summary>
/// Utility class for generating blob storage paths with unique filenames.
/// </summary>
public static class BlobPathHelper
{
    /// <summary>
    /// Generates a unique filename with a timestamp prefix.
    /// </summary>
    /// <param name="originalFileName">The original filename to preserve.</param>
    /// <param name="index">Optional index for batch uploads (e.g., multiple photos).</param>
    /// <returns>A unique filename in the format: {timestamp}_{index}_{originalFileName} or {timestamp}_{originalFileName}</returns>
    public static string GenerateUniqueFileName(string originalFileName, int? index = null)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var safeFileName = Path.GetFileName(originalFileName);

        return index.HasValue
            ? $"{timestamp}_{index.Value}_{safeFileName}"
            : $"{timestamp}_{safeFileName}";
    }

    /// <summary>
    /// Generates a unique filename for a signature file.
    /// </summary>
    /// <returns>A unique signature filename in the format: {timestamp}_signature.png</returns>
    public static string GenerateSignatureFileName()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{timestamp}_signature.png";
    }

    /// <summary>
    /// Generates a unique filename for a placeholder JSON file.
    /// </summary>
    /// <param name="prefix">The prefix for the placeholder file (e.g., "pod", "bol").</param>
    /// <returns>A unique placeholder filename in the format: {timestamp}_{prefix}.json</returns>
    public static string GeneratePlaceholderFileName(string prefix)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{timestamp}_{prefix}.json";
    }

    /// <summary>
    /// Generates a blob path for load-related documents.
    /// </summary>
    /// <param name="loadId">The load ID.</param>
    /// <param name="folder">The subfolder (e.g., "pod", "bol", "inspection").</param>
    /// <param name="fileName">The filename.</param>
    /// <returns>A blob path in the format: loads/{loadId}/{folder}/{fileName}</returns>
    public static string GetLoadBlobPath(Guid loadId, string folder, string fileName) => $"loads/{loadId}/{folder}/{fileName}";

    /// <summary>
    /// Generates a blob path for owner-specific documents.
    /// </summary>
    /// <param name="ownerSegment">The owner type segment (e.g., "loads", "employees", "trucks").</param>
    /// <param name="ownerId">The owner ID.</param>
    /// <param name="fileName">The filename.</param>
    /// <returns>A blob path in the format: {ownerSegment}/{ownerId}/documents/{fileName}</returns>
    public static string GetOwnerDocumentBlobPath(string ownerSegment, Guid ownerId, string fileName) => $"{ownerSegment}/{ownerId}/documents/{fileName}";

    /// <summary>
    /// Generates a blob path for expense receipts organized by date.
    /// </summary>
    /// <param name="fileName">The filename.</param>
    /// <returns>A blob path in the format: {year}/{month}/{fileName}</returns>
    public static string GetReceiptBlobPath(string fileName) => $"{DateTime.UtcNow:yyyy/MM}/{fileName}";
}
