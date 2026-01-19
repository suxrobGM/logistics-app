package com.jfleets.driver.ui.components.inspection

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.Card
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.jfleets.driver.ui.components.VehicleDiagram
import com.jfleets.driver.viewmodel.DamageMarker

/**
 * Section for displaying vehicle diagram and damage markers.
 */
@Composable
fun DamageMarkersSection(
    damageMarkers: List<DamageMarker>,
    onDiagramTap: (x: Double, y: Double) -> Unit,
    onRemoveMarker: (index: Int) -> Unit,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        Text(
            text = "Damage Markers",
            style = MaterialTheme.typography.titleMedium
        )

        Text(
            text = "Tap on the vehicle diagram to mark damage",
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )

        Spacer(modifier = Modifier.height(8.dp))

        VehicleDiagram(
            damageMarkers = damageMarkers,
            onTap = onDiagramTap
        )

        if (damageMarkers.isNotEmpty()) {
            Spacer(modifier = Modifier.height(16.dp))

            Text(
                text = "Marked Damage (${damageMarkers.size})",
                style = MaterialTheme.typography.titleSmall
            )

            Spacer(modifier = Modifier.height(8.dp))

            damageMarkers.forEachIndexed { index, marker ->
                DamageMarkerItem(
                    index = index,
                    marker = marker,
                    onRemove = { onRemoveMarker(index) }
                )
                if (index < damageMarkers.lastIndex) {
                    Spacer(modifier = Modifier.height(8.dp))
                }
            }
        }
    }
}

@Composable
private fun DamageMarkerItem(
    index: Int,
    marker: DamageMarker,
    onRemove: () -> Unit
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = "Damage #${index + 1}",
                    style = MaterialTheme.typography.bodyMedium
                )
                if (!marker.description.isNullOrBlank()) {
                    Text(
                        text = marker.description,
                        style = MaterialTheme.typography.bodySmall
                    )
                }
                if (!marker.severity.isNullOrBlank()) {
                    Text(
                        text = "Severity: ${marker.severity}",
                        style = MaterialTheme.typography.labelSmall,
                        color = getSeverityColor(marker.severity)
                    )
                }
            }
            IconButton(onClick = onRemove) {
                Icon(
                    Icons.Default.Close,
                    contentDescription = "Remove",
                    tint = MaterialTheme.colorScheme.error
                )
            }
        }
    }
}

private fun getSeverityColor(severity: String): Color = when (severity.lowercase()) {
    "severe" -> Color.Red
    "moderate" -> Color(0xFFFFA500)
    else -> Color(0xFFCCAA00) // Darker yellow for better visibility
}
