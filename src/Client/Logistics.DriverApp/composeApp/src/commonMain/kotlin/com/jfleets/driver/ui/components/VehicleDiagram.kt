package com.jfleets.driver.ui.components

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.unit.dp
import com.jfleets.driver.viewmodel.DamageMarker

@Composable
fun VehicleDiagram(
    damageMarkers: List<DamageMarker>,
    onTap: (x: Double, y: Double) -> Unit,
    modifier: Modifier = Modifier
) {
    Box(
        modifier = modifier
            .fillMaxWidth()
            .aspectRatio(2f)
            .border(
                width = 1.dp,
                color = MaterialTheme.colorScheme.outline,
                shape = RoundedCornerShape(8.dp)
            )
            .background(
                color = Color.White,
                shape = RoundedCornerShape(8.dp)
            )
    ) {
        Canvas(
            modifier = Modifier
                .fillMaxSize()
                .pointerInput(Unit) {
                    detectTapGestures { offset ->
                        val x = offset.x / size.width.toDouble()
                        val y = offset.y / size.height.toDouble()
                        onTap(x, y)
                    }
                }
        ) {
            val width = size.width
            val height = size.height
            val padding = 20f

            // Draw car silhouette (top-down view)
            val carPath = Path().apply {
                // Car body outline
                val carLeft = padding + width * 0.15f
                val carRight = width - padding - width * 0.15f
                val carTop = padding + height * 0.1f
                val carBottom = height - padding - height * 0.1f
                val carWidth = carRight - carLeft
                val carHeight = carBottom - carTop

                // Front (rounded)
                moveTo(carLeft + carWidth * 0.2f, carTop)
                quadraticTo(carLeft, carTop, carLeft, carTop + carHeight * 0.15f)

                // Left side
                lineTo(carLeft, carBottom - carHeight * 0.15f)

                // Rear left (rounded)
                quadraticTo(carLeft, carBottom, carLeft + carWidth * 0.1f, carBottom)

                // Rear
                lineTo(carRight - carWidth * 0.1f, carBottom)

                // Rear right (rounded)
                quadraticTo(carRight, carBottom, carRight, carBottom - carHeight * 0.15f)

                // Right side
                lineTo(carRight, carTop + carHeight * 0.15f)

                // Front right (rounded)
                quadraticTo(carRight, carTop, carRight - carWidth * 0.2f, carTop)

                close()
            }

            // Draw car outline
            drawPath(
                path = carPath,
                color = Color.Gray,
                style = Stroke(width = 2.dp.toPx())
            )

            // Draw wheels
            val wheelWidth = width * 0.08f
            val wheelHeight = height * 0.12f
            val carLeft = padding + width * 0.15f
            val carRight = width - padding - width * 0.15f
            val carTop = padding + height * 0.1f
            val carBottom = height - padding - height * 0.1f

            // Front left wheel
            drawRect(
                color = Color.DarkGray,
                topLeft = Offset(carLeft - wheelWidth / 2, carTop + height * 0.1f),
                size = Size(wheelWidth, wheelHeight)
            )
            // Front right wheel
            drawRect(
                color = Color.DarkGray,
                topLeft = Offset(carRight - wheelWidth / 2, carTop + height * 0.1f),
                size = Size(wheelWidth, wheelHeight)
            )
            // Rear left wheel
            drawRect(
                color = Color.DarkGray,
                topLeft = Offset(carLeft - wheelWidth / 2, carBottom - height * 0.1f - wheelHeight),
                size = Size(wheelWidth, wheelHeight)
            )
            // Rear right wheel
            drawRect(
                color = Color.DarkGray,
                topLeft = Offset(
                    carRight - wheelWidth / 2,
                    carBottom - height * 0.1f - wheelHeight
                ),
                size = Size(wheelWidth, wheelHeight)
            )

            // Draw windshield
            val windshieldPath = Path().apply {
                val wsLeft = carLeft + (carRight - carLeft) * 0.2f
                val wsRight = carRight - (carRight - carLeft) * 0.2f
                val wsTop = carTop + height * 0.08f
                val wsBottom = carTop + height * 0.25f

                moveTo(wsLeft, wsTop)
                lineTo(wsRight, wsTop)
                lineTo(wsRight - (carRight - carLeft) * 0.05f, wsBottom)
                lineTo(wsLeft + (carRight - carLeft) * 0.05f, wsBottom)
                close()
            }
            drawPath(
                path = windshieldPath,
                color = Color.LightGray.copy(alpha = 0.5f)
            )

            // Draw damage markers
            damageMarkers.forEachIndexed { index, marker ->
                val markerX = (marker.x * width).toFloat()
                val markerY = (marker.y * height).toFloat()
                val markerRadius = 12.dp.toPx()

                // Marker color based on severity
                val markerColor = when (marker.severity?.lowercase()) {
                    "severe" -> Color.Red
                    "moderate" -> Color(0xFFFFA500) // Orange
                    else -> Color.Yellow
                }

                // Draw marker
                drawCircle(
                    color = markerColor,
                    radius = markerRadius,
                    center = Offset(markerX, markerY)
                )
                drawCircle(
                    color = Color.Black,
                    radius = markerRadius,
                    center = Offset(markerX, markerY),
                    style = Stroke(width = 2.dp.toPx())
                )
            }
        }
    }
}
