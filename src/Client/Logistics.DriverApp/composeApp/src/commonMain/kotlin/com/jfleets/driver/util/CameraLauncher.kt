package com.jfleets.driver.util

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
 * Multiplatform camera launcher interface.
 * Provides photo capture functionality using platform-specific implementations.
 *
 * Uses platform-specific implementations:
 * - Android: ActivityResultContracts.TakePicture() or CameraX
 * - iOS: UIImagePickerController
 */
expect class CameraLauncher {
    /**
     * Launches the camera to capture a photo.
     *
     * @param onResult Callback with the captured photo result, or null if cancelled/failed
     */
    fun launchCamera(onResult: (CaptureResult?) -> Unit)

    /**
     * Launches the photo picker to select from gallery.
     *
     * @param onResult Callback with the selected photo result, or null if cancelled/failed
     */
    fun launchGallery(onResult: (CaptureResult?) -> Unit)
}
