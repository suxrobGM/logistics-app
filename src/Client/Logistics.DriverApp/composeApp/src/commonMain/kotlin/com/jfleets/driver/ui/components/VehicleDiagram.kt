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
import androidx.compose.ui.geometry.CornerRadius
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
            .aspectRatio(0.7f) // Taller aspect ratio for better car proportions
            .border(
                width = 1.dp,
                color = MaterialTheme.colorScheme.outline,
                shape = RoundedCornerShape(8.dp)
            )
            .background(
                color = Color(0xFFF5F5F5),
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
            val padding = width * 0.15f

            // Car dimensions - centered in canvas
            val carWidth = width * 0.5f
            val carHeight = height * 0.85f
            val carLeft = (width - carWidth) / 2
            val carRight = carLeft + carWidth
            val carTop = (height - carHeight) / 2
            val carBottom = carTop + carHeight

            // Hood taper and corner radius
            val hoodTaper = carWidth * 0.08f
            val bodyCorner = carWidth * 0.12f
            val hoodCorner = carWidth * 0.08f

            // Draw car body - clean sedan shape
            val bodyPath = Path().apply {
                // Start at front left after hood taper
                moveTo(carLeft + hoodTaper, carTop + hoodCorner)

                // Front left corner (hood - narrower)
                quadraticTo(
                    carLeft + hoodTaper, carTop,
                    carLeft + hoodTaper + hoodCorner, carTop
                )

                // Front edge (hood)
                lineTo(carRight - hoodTaper - hoodCorner, carTop)

                // Front right corner (hood)
                quadraticTo(
                    carRight - hoodTaper, carTop,
                    carRight - hoodTaper, carTop + hoodCorner
                )

                // Transition from hood to body (right side)
                lineTo(carRight - hoodTaper, carTop + carHeight * 0.12f)
                quadraticTo(
                    carRight, carTop + carHeight * 0.14f,
                    carRight, carTop + carHeight * 0.18f
                )

                // Right side (straight)
                lineTo(carRight, carBottom - carHeight * 0.12f)

                // Rear right corner
                quadraticTo(
                    carRight, carBottom,
                    carRight - bodyCorner, carBottom
                )

                // Rear edge
                lineTo(carLeft + bodyCorner, carBottom)

                // Rear left corner
                quadraticTo(
                    carLeft, carBottom,
                    carLeft, carBottom - carHeight * 0.12f
                )

                // Left side (straight)
                lineTo(carLeft, carTop + carHeight * 0.18f)

                // Transition from body to hood (left side)
                quadraticTo(
                    carLeft, carTop + carHeight * 0.14f,
                    carLeft + hoodTaper, carTop + carHeight * 0.12f
                )

                close()
            }

            // Car body fill
            drawPath(path = bodyPath, color = Color(0xFFDDDDDD))

            // Car body outline
            drawPath(
                path = bodyPath,
                color = Color(0xFF555555),
                style = Stroke(width = 2.5f.dp.toPx())
            )

            // Cabin area (darker to show roof)
            val cabinPath = Path().apply {
                val cabinTop = carTop + carHeight * 0.22f
                val cabinBottom = carBottom - carHeight * 0.18f
                val cabinLeft = carLeft + carWidth * 0.08f
                val cabinRight = carRight - carWidth * 0.08f
                val cabinCorner = carWidth * 0.06f

                moveTo(cabinLeft + cabinCorner, cabinTop)
                lineTo(cabinRight - cabinCorner, cabinTop)
                quadraticTo(cabinRight, cabinTop, cabinRight, cabinTop + cabinCorner)
                lineTo(cabinRight, cabinBottom - cabinCorner)
                quadraticTo(cabinRight, cabinBottom, cabinRight - cabinCorner, cabinBottom)
                lineTo(cabinLeft + cabinCorner, cabinBottom)
                quadraticTo(cabinLeft, cabinBottom, cabinLeft, cabinBottom - cabinCorner)
                lineTo(cabinLeft, cabinTop + cabinCorner)
                quadraticTo(cabinLeft, cabinTop, cabinLeft + cabinCorner, cabinTop)
                close()
            }
            drawPath(path = cabinPath, color = Color(0xFFCCCCCC))

            // Front windshield
            val windshieldPath = Path().apply {
                val wsTop = carTop + carHeight * 0.14f
                val wsBottom = carTop + carHeight * 0.26f
                val wsInset = carWidth * 0.12f

                moveTo(carLeft + wsInset + carWidth * 0.03f, wsTop)
                lineTo(carRight - wsInset - carWidth * 0.03f, wsTop)
                lineTo(carRight - wsInset, wsBottom)
                lineTo(carLeft + wsInset, wsBottom)
                close()
            }
            drawPath(path = windshieldPath, color = Color(0xFFADD8E6))
            drawPath(
                path = windshieldPath,
                color = Color(0xFF777777),
                style = Stroke(width = 1.dp.toPx())
            )

            // Rear windshield
            val rearWindowPath = Path().apply {
                val rwTop = carBottom - carHeight * 0.22f
                val rwBottom = carBottom - carHeight * 0.10f
                val rwInset = carWidth * 0.12f

                moveTo(carLeft + rwInset, rwTop)
                lineTo(carRight - rwInset, rwTop)
                lineTo(carRight - rwInset - carWidth * 0.02f, rwBottom)
                lineTo(carLeft + rwInset + carWidth * 0.02f, rwBottom)
                close()
            }
            drawPath(path = rearWindowPath, color = Color(0xFFADD8E6))
            drawPath(
                path = rearWindowPath,
                color = Color(0xFF777777),
                style = Stroke(width = 1.dp.toPx())
            )

            // Wheels - dark with rounded corners
            val wheelWidth = carWidth * 0.22f
            val wheelHeight = carHeight * 0.10f
            val wheelColor = Color(0xFF2D2D2D)
            val wheelCorner = CornerRadius(4.dp.toPx())

            // Front left wheel
            drawRoundRect(
                color = wheelColor,
                topLeft = Offset(carLeft - wheelWidth * 0.35f, carTop + carHeight * 0.08f),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )

            // Front right wheel
            drawRoundRect(
                color = wheelColor,
                topLeft = Offset(carRight - wheelWidth * 0.65f, carTop + carHeight * 0.08f),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )

            // Rear left wheel
            drawRoundRect(
                color = wheelColor,
                topLeft = Offset(carLeft - wheelWidth * 0.35f, carBottom - carHeight * 0.08f - wheelHeight),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )

            // Rear right wheel
            drawRoundRect(
                color = wheelColor,
                topLeft = Offset(carRight - wheelWidth * 0.65f, carBottom - carHeight * 0.08f - wheelHeight),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )

            // Headlights
            val lightWidth = carWidth * 0.14f
            val lightHeight = carHeight * 0.025f
            val lightCorner = CornerRadius(2.dp.toPx())

            drawRoundRect(
                color = Color(0xFFFFEB3B),
                topLeft = Offset(carLeft + hoodTaper + carWidth * 0.05f, carTop + carHeight * 0.015f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )
            drawRoundRect(
                color = Color(0xFFFFEB3B),
                topLeft = Offset(carRight - hoodTaper - carWidth * 0.05f - lightWidth, carTop + carHeight * 0.015f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )

            // Taillights
            drawRoundRect(
                color = Color(0xFFE53935),
                topLeft = Offset(carLeft + carWidth * 0.08f, carBottom - carHeight * 0.035f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )
            drawRoundRect(
                color = Color(0xFFE53935),
                topLeft = Offset(carRight - carWidth * 0.08f - lightWidth, carBottom - carHeight * 0.035f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )

            // Side mirrors
            val mirrorWidth = carWidth * 0.10f
            val mirrorHeight = carHeight * 0.018f

            drawRoundRect(
                color = Color(0xFF555555),
                topLeft = Offset(carLeft - mirrorWidth + carWidth * 0.02f, carTop + carHeight * 0.20f),
                size = Size(mirrorWidth, mirrorHeight),
                cornerRadius = CornerRadius(2.dp.toPx())
            )
            drawRoundRect(
                color = Color(0xFF555555),
                topLeft = Offset(carRight - carWidth * 0.02f, carTop + carHeight * 0.20f),
                size = Size(mirrorWidth, mirrorHeight),
                cornerRadius = CornerRadius(2.dp.toPx())
            )

            // Draw damage markers
            damageMarkers.forEach { marker ->
                val markerX = (marker.x * width).toFloat()
                val markerY = (marker.y * height).toFloat()
                val markerRadius = 12.dp.toPx()

                val markerColor = when (marker.severity?.lowercase()) {
                    "severe" -> Color(0xFFD32F2F)
                    "moderate" -> Color(0xFFFF9800)
                    else -> Color(0xFFFFC107)
                }

                // Shadow
                drawCircle(
                    color = Color.Black.copy(alpha = 0.25f),
                    radius = markerRadius + 2.dp.toPx(),
                    center = Offset(markerX + 2.dp.toPx(), markerY + 2.dp.toPx())
                )

                // Marker fill
                drawCircle(
                    color = markerColor,
                    radius = markerRadius,
                    center = Offset(markerX, markerY)
                )

                // Marker border
                drawCircle(
                    color = Color.White,
                    radius = markerRadius,
                    center = Offset(markerX, markerY),
                    style = Stroke(width = 2.5f.dp.toPx())
                )
            }
        }
    }
}
