package com.jfleets.driver.util

import com.jfleets.driver.ui.components.PathData
import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.cinterop.addressOf
import kotlinx.cinterop.usePinned
import platform.CoreGraphics.CGPointMake
import platform.CoreGraphics.CGRectMake
import platform.CoreGraphics.CGSizeMake
import platform.Foundation.NSData
import platform.Foundation.base64EncodedStringWithOptions
import platform.UIKit.UIBezierPath
import platform.UIKit.UIColor
import platform.UIKit.UIGraphicsImageRenderer
import platform.UIKit.UIGraphicsImageRendererFormat
import platform.UIKit.UIImagePNGRepresentation

/**
 * iOS implementation using UIGraphicsImageRenderer.
 */
actual object SignatureConverter {
    @OptIn(ExperimentalForeignApi::class)
    actual fun pathsToBase64Png(
        paths: List<PathData>,
        width: Int,
        height: Int,
        strokeWidth: Float
    ): String? {
        if (paths.isEmpty()) return null

        return try {
            val size = CGSizeMake(width.toDouble(), height.toDouble())
            val format = UIGraphicsImageRendererFormat().apply {
                scale = 1.0
            }
            val renderer = UIGraphicsImageRenderer(size = size, format = format)

            val image = renderer.imageWithActions { context ->
                // Fill white background
                UIColor.whiteColor.setFill()
                context?.fillRect(CGRectMake(0.0, 0.0, width.toDouble(), height.toDouble()))

                // Configure stroke
                UIColor.blackColor.setStroke()

                // Draw each path
                for (pathData in paths) {
                    if (pathData.points.size < 2) continue

                    val bezierPath = UIBezierPath()
                    bezierPath.lineWidth = strokeWidth.toDouble()
                    bezierPath.lineCapStyle = platform.CoreGraphics.kCGLineCapRound
                    bezierPath.lineJoinStyle = platform.CoreGraphics.kCGLineJoinRound

                    val firstPoint = pathData.points.first()
                    bezierPath.moveToPoint(CGPointMake(firstPoint.x.toDouble(), firstPoint.y.toDouble()))

                    for (i in 1 until pathData.points.size) {
                        val point = pathData.points[i]
                        bezierPath.addLineToPoint(CGPointMake(point.x.toDouble(), point.y.toDouble()))
                    }

                    bezierPath.stroke()
                }
            }

            // Convert to PNG data
            val pngData: NSData? = UIImagePNGRepresentation(image)
            if (pngData == null) {
                Logger.e("SignatureConverter", "Failed to create PNG data")
                return null
            }

            // Encode to Base64
            pngData.base64EncodedStringWithOptions(0u)
        } catch (e: Exception) {
            Logger.e("SignatureConverter", "Failed to convert signature to Base64: ${e.message}")
            null
        }
    }
}
