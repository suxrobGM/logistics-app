package com.jfleets.driver.util

import com.jfleets.driver.ui.components.PathData

/**
 * Multiplatform signature converter utility.
 * Converts drawn signature paths to Base64-encoded PNG images.
 *
 * Uses platform-specific implementations:
 * - Android: android.graphics.Bitmap + Canvas
 * - iOS: UIGraphicsImageRenderer
 */
expect object SignatureConverter {
    /**
     * Converts signature paths to a Base64-encoded PNG image.
     *
     * @param paths List of PathData containing signature stroke points
     * @param width Width of the output image in pixels
     * @param height Height of the output image in pixels
     * @param strokeWidth Width of the signature strokes in pixels
     * @return Base64-encoded PNG image string, or null if conversion fails
     */
    fun pathsToBase64Png(
        paths: List<PathData>,
        width: Int = 400,
        height: Int = 200,
        strokeWidth: Float = 3f
    ): String?
}
