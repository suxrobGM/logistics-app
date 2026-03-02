package com.logisticsx.driver.ui.components

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
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.ui.theme.LocalExtendedColors
import com.logisticsx.driver.ui.theme.Radius
import com.logisticsx.driver.viewmodel.DamageMarker

@Composable
fun VehicleDiagram(
    damageMarkers: List<DamageMarker>,
    onTap: (x: Double, y: Double) -> Unit,
    modifier: Modifier = Modifier
) {
    val extendedColors = LocalExtendedColors.current
    val markerDescription = if (damageMarkers.isEmpty()) {
        "Vehicle damage diagram. Tap to mark damage location."
    } else {
        "Vehicle damage diagram with ${damageMarkers.size} damage markers. Tap to add more."
    }

    Box(
        modifier = modifier
            .fillMaxWidth()
            .aspectRatio(0.7f)
            .border(
                width = 1.dp,
                color = MaterialTheme.colorScheme.outline,
                shape = RoundedCornerShape(Radius.md)
            )
            .background(
                color = extendedColors.diagramBackground,
                shape = RoundedCornerShape(Radius.md)
            )
            .semantics { contentDescription = markerDescription }
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
                moveTo(carLeft + hoodTaper, carTop + hoodCorner)
                quadraticTo(
                    carLeft + hoodTaper, carTop,
                    carLeft + hoodTaper + hoodCorner, carTop
                )
                lineTo(carRight - hoodTaper - hoodCorner, carTop)
                quadraticTo(
                    carRight - hoodTaper, carTop,
                    carRight - hoodTaper, carTop + hoodCorner
                )
                lineTo(carRight - hoodTaper, carTop + carHeight * 0.12f)
                quadraticTo(
                    carRight, carTop + carHeight * 0.14f,
                    carRight, carTop + carHeight * 0.18f
                )
                lineTo(carRight, carBottom - carHeight * 0.12f)
                quadraticTo(
                    carRight, carBottom,
                    carRight - bodyCorner, carBottom
                )
                lineTo(carLeft + bodyCorner, carBottom)
                quadraticTo(
                    carLeft, carBottom,
                    carLeft, carBottom - carHeight * 0.12f
                )
                lineTo(carLeft, carTop + carHeight * 0.18f)
                quadraticTo(
                    carLeft, carTop + carHeight * 0.14f,
                    carLeft + hoodTaper, carTop + carHeight * 0.12f
                )
                close()
            }

            drawPath(path = bodyPath, color = extendedColors.diagramBodyFill)
            drawPath(
                path = bodyPath,
                color = extendedColors.diagramBodyOutline,
                style = Stroke(width = 2.5f.dp.toPx())
            )

            // Cabin area
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
            drawPath(path = cabinPath, color = extendedColors.diagramCabin)

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
            drawPath(path = windshieldPath, color = extendedColors.diagramGlass)
            drawPath(
                path = windshieldPath,
                color = extendedColors.diagramGlassOutline,
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
            drawPath(path = rearWindowPath, color = extendedColors.diagramGlass)
            drawPath(
                path = rearWindowPath,
                color = extendedColors.diagramGlassOutline,
                style = Stroke(width = 1.dp.toPx())
            )

            // Wheels
            val wheelWidth = carWidth * 0.22f
            val wheelHeight = carHeight * 0.10f
            val wheelCorner = CornerRadius(4.dp.toPx())

            drawRoundRect(
                color = extendedColors.diagramWheel,
                topLeft = Offset(carLeft - wheelWidth * 0.35f, carTop + carHeight * 0.08f),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )
            drawRoundRect(
                color = extendedColors.diagramWheel,
                topLeft = Offset(carRight - wheelWidth * 0.65f, carTop + carHeight * 0.08f),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )
            drawRoundRect(
                color = extendedColors.diagramWheel,
                topLeft = Offset(carLeft - wheelWidth * 0.35f, carBottom - carHeight * 0.08f - wheelHeight),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )
            drawRoundRect(
                color = extendedColors.diagramWheel,
                topLeft = Offset(carRight - wheelWidth * 0.65f, carBottom - carHeight * 0.08f - wheelHeight),
                size = Size(wheelWidth, wheelHeight),
                cornerRadius = wheelCorner
            )

            // Headlights
            val lightWidth = carWidth * 0.14f
            val lightHeight = carHeight * 0.025f
            val lightCorner = CornerRadius(2.dp.toPx())

            drawRoundRect(
                color = extendedColors.diagramHeadlight,
                topLeft = Offset(carLeft + hoodTaper + carWidth * 0.05f, carTop + carHeight * 0.015f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )
            drawRoundRect(
                color = extendedColors.diagramHeadlight,
                topLeft = Offset(carRight - hoodTaper - carWidth * 0.05f - lightWidth, carTop + carHeight * 0.015f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )

            // Taillights
            drawRoundRect(
                color = extendedColors.diagramTaillight,
                topLeft = Offset(carLeft + carWidth * 0.08f, carBottom - carHeight * 0.035f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )
            drawRoundRect(
                color = extendedColors.diagramTaillight,
                topLeft = Offset(carRight - carWidth * 0.08f - lightWidth, carBottom - carHeight * 0.035f),
                size = Size(lightWidth, lightHeight),
                cornerRadius = lightCorner
            )

            // Side mirrors
            val mirrorWidth = carWidth * 0.10f
            val mirrorHeight = carHeight * 0.018f

            drawRoundRect(
                color = extendedColors.diagramMirror,
                topLeft = Offset(carLeft - mirrorWidth + carWidth * 0.02f, carTop + carHeight * 0.20f),
                size = Size(mirrorWidth, mirrorHeight),
                cornerRadius = CornerRadius(2.dp.toPx())
            )
            drawRoundRect(
                color = extendedColors.diagramMirror,
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
                    "severe" -> extendedColors.damageSevere
                    "moderate" -> extendedColors.damageModerate
                    else -> extendedColors.damageMinor
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
