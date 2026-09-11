package com.logisticsx.driver.util

/**
 * Result of a camera capture operation.
 */
data class CaptureResult(
    val bytes: ByteArray,
    val fileName: String,
    val contentType: String = "image/jpeg"
) {
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other == null || this::class != other::class) return false
        other as CaptureResult
        if (!bytes.contentEquals(other.bytes)) return false
        if (fileName != other.fileName) return false
        if (contentType != other.contentType) return false
        return true
    }

    override fun hashCode(): Int {
        var result = bytes.contentHashCode()
        result = 31 * result + fileName.hashCode()
        result = 31 * result + contentType.hashCode()
        return result
    }
}

/**
 * Photo capture, backed by the Activity Result API on Android and UIImagePickerController on iOS.
 */
expect class CameraLauncher {
    /** Captures a photo. Reports null if the user cancels or capture fails. */
    fun launchCamera(onResult: (CaptureResult?) -> Unit)

    /** Picks a photo from the gallery. Reports null if the user cancels or the pick fails. */
    fun launchGallery(onResult: (CaptureResult?) -> Unit)
}
