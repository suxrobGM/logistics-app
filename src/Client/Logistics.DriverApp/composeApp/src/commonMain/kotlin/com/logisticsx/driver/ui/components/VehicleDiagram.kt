package com.logisticsx.driver.ui.components

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.Image
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
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.ui.theme.LocalExtendedColors
import com.logisticsx.driver.ui.theme.Radius
import com.logisticsx.driver.viewmodel.DamageMarker
import logisticsdriver.composeapp.generated.resources.Res
import logisticsdriver.composeapp.generated.resources.inspection_truck_side
import org.jetbrains.compose.resources.painterResource

private const val VehicleBoundsLeft = 0.04f
private const val VehicleBoundsRight = 0.97f
private const val VehicleBoundsTop = 0.31f
private const val VehicleBoundsBottom = 0.69f

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
            .aspectRatio(16f / 9f)
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
        Image(
            painter = painterResource(Res.drawable.inspection_truck_side),
            contentDescription = null,
            modifier = Modifier.fillMaxSize(),
            contentScale = ContentScale.Fit
        )

        Canvas(
            modifier = Modifier
                .fillMaxSize()
                .pointerInput(Unit) {
                    detectTapGestures { offset ->
                        val x = normalizeFromVehicleBounds(
                            value = offset.x / size.width,
                            min = VehicleBoundsLeft,
                            max = VehicleBoundsRight
                        )
                        val y = normalizeFromVehicleBounds(
                            value = offset.y / size.height,
                            min = VehicleBoundsTop,
                            max = VehicleBoundsBottom
                        )
                        onTap(x, y)
                    }
                }
        ) {
            damageMarkers.forEach { marker ->
                val markerX = mapToVehicleBounds(
                    value = marker.x,
                    min = VehicleBoundsLeft,
                    max = VehicleBoundsRight
                ) * size.width
                val markerY = mapToVehicleBounds(
                    value = marker.y,
                    min = VehicleBoundsTop,
                    max = VehicleBoundsBottom
                ) * size.height
                val markerRadius = 12.dp.toPx()

                val markerColor = when (marker.severity?.lowercase()) {
                    "severe" -> extendedColors.damageSevere
                    "moderate" -> extendedColors.damageModerate
                    else -> extendedColors.damageMinor
                }

                drawCircle(
                    color = Color.Black.copy(alpha = 0.25f),
                    radius = markerRadius + 2.dp.toPx(),
                    center = Offset(markerX + 2.dp.toPx(), markerY + 2.dp.toPx())
                )
                drawCircle(
                    color = markerColor,
                    radius = markerRadius,
                    center = Offset(markerX, markerY)
                )
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

private fun mapToVehicleBounds(value: Double, min: Float, max: Float): Float {
    return min + value.coerceIn(0.0, 1.0).toFloat() * (max - min)
}

private fun normalizeFromVehicleBounds(value: Float, min: Float, max: Float): Double {
    return ((value - min) / (max - min)).coerceIn(0f, 1f).toDouble()
}
