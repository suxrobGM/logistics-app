package com.jfleets.driver.util

/**
 * Result of a barcode scan operation.
 */
data class ScanResult(
    val value: String,
    val format: BarcodeFormat
)

/**
 * Supported barcode formats for scanning.
 */
enum class BarcodeFormat {
    CODE_39,
    CODE_128,
    DATA_MATRIX,
    QR_CODE,
    UNKNOWN
}

/**
 * Multiplatform barcode scanner launcher interface.
 * Provides barcode/VIN scanning functionality using platform-specific implementations.
 *
 * Uses platform-specific implementations:
 * - Android: ML Kit Barcode Scanning or ZXing
 * - iOS: AVFoundation barcode scanning
 */
expect class BarcodeScannerLauncher {
    /**
     * Launches the barcode scanner to scan a VIN or other barcode.
     *
     * @param onResult Callback with the scanned barcode result, or null if cancelled/failed
     */
    fun launchScanner(onResult: (ScanResult?) -> Unit)
}
