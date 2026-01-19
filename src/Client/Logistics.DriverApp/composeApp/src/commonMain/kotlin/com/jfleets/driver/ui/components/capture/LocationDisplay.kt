package com.jfleets.driver.ui.components.capture

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import kotlin.math.round

/**
 * Displays GPS coordinates.
 */
@Composable
fun LocationDisplay(
    latitude: Double,
    longitude: Double,
    modifier: Modifier = Modifier
) {
    Text(
        text = "Location: ${formatCoordinate(latitude)}, ${formatCoordinate(longitude)}",
        style = MaterialTheme.typography.bodySmall,
        color = MaterialTheme.colorScheme.onSurfaceVariant,
        modifier = modifier
    )
}

/**
 * Formats a coordinate value to a string with up to 6 decimal places.
 */
private fun formatCoordinate(value: Double): String {
    val rounded = round(value * 1000000.0) / 1000000.0
    return rounded.toString()
}
