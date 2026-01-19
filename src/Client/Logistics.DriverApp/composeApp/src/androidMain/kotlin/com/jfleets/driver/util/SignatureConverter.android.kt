package com.jfleets.driver.util

import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.Path
import android.util.Base64
import com.jfleets.driver.ui.components.PathData
import java.io.ByteArrayOutputStream

/**
 * Android implementation using android.graphics.Bitmap and Canvas.
 */
actual object SignatureConverter {
    actual fun pathsToBase64Png(
        paths: List<PathData>,
        width: Int,
        height: Int,
        strokeWidth: Float
    ): String? {
        if (paths.isEmpty()) return null

        return try {
            // Create bitmap with white background
            val bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888)
            val canvas = Canvas(bitmap)
            canvas.drawColor(Color.WHITE)

            // Configure paint for signature strokes
            val paint = Paint().apply {
                color = Color.BLACK
                style = Paint.Style.STROKE
                this.strokeWidth = strokeWidth
                strokeCap = Paint.Cap.ROUND
                strokeJoin = Paint.Join.ROUND
                isAntiAlias = true
            }

            // Draw each path
            for (pathData in paths) {
                if (pathData.points.size < 2) continue

                val path = Path()
                val firstPoint = pathData.points.first()
                path.moveTo(firstPoint.x, firstPoint.y)

                for (i in 1 until pathData.points.size) {
                    val point = pathData.points[i]
                    path.lineTo(point.x, point.y)
                }

                canvas.drawPath(path, paint)
            }

            // Convert to PNG bytes
            val outputStream = ByteArrayOutputStream()
            bitmap.compress(Bitmap.CompressFormat.PNG, 100, outputStream)
            val pngBytes = outputStream.toByteArray()

            // Clean up
            bitmap.recycle()

            // Encode to Base64
            Base64.encodeToString(pngBytes, Base64.NO_WRAP)
        } catch (e: Exception) {
            Logger.e("SignatureConverter", "Failed to convert signature to Base64", e)
            null
        }
    }
}
