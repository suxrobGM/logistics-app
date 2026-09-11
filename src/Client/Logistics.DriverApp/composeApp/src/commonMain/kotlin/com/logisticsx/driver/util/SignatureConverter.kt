package com.logisticsx.driver.util

import com.logisticsx.driver.ui.components.PathData

/**
 * Converts drawn signature paths to Base64-encoded PNG images.
 *
 * Android draws with Bitmap and Canvas, iOS with UIGraphicsImageRenderer.
 */
expect object SignatureConverter {
    /**
     * Renders signature strokes to a Base64-encoded PNG, or null if rendering fails.
     *
     * All sizes are in pixels.
     */
    fun pathsToBase64Png(
        paths: List<PathData>,
        width: Int = 400,
        height: Int = 200,
        strokeWidth: Float = 3f
    ): String?
}
