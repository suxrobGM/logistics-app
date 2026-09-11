package com.logisticsx.driver.util

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
 * Barcode and VIN scanning, backed by ML Kit on Android and AVFoundation on iOS.
 */
expect class BarcodeScannerLauncher {
    /** Scans a VIN or other barcode. Reports null if the user cancels or the scan fails. */
    fun launchScanner(onResult: (ScanResult?) -> Unit)
}
