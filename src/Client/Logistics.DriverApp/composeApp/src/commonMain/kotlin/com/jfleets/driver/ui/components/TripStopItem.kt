package com.jfleets.driver.ui.components

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.filled.Map
import androidx.compose.material.icons.filled.Place
import androidx.compose.material.icons.filled.Schedule
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.TripStopDto
import com.jfleets.driver.api.models.TripStopType
import com.jfleets.driver.model.toDisplayString
import com.jfleets.driver.util.formatShort

/**
 * Displays a single trip stop with its status, address, and navigation button.
 */
@Composable
fun TripStopItem(
    stop: TripStopDto,
    stopNumber: Int,
    onNavigateClick: () -> Unit = {}
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.Top
    ) {
        // Stop icon based on type and arrival status
        val isArrived = stop.arrivedAt != null
        val icon = if (isArrived) Icons.Default.CheckCircle else Icons.Default.Place
        val iconTint = when {
            isArrived -> MaterialTheme.colorScheme.secondary
            stop.type == TripStopType.PICK_UP -> MaterialTheme.colorScheme.primary
            else -> MaterialTheme.colorScheme.tertiary
        }

        Icon(
            imageVector = icon,
            contentDescription = null,
            tint = iconTint,
            modifier = Modifier.size(32.dp)
        )

        Spacer(modifier = Modifier.width(12.dp))

        Column(modifier = Modifier.weight(1f)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Text(
                    text = "Stop $stopNumber: ${if (stop.type == TripStopType.PICK_UP) "Pick Up" else "Drop Off"}",
                    style = MaterialTheme.typography.bodyLarge,
                    fontWeight = FontWeight.Medium
                )
            }
            Spacer(modifier = Modifier.height(4.dp))
            Text(
                text = stop.address.toDisplayString(),
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )

            // Show arrival time if arrived
            stop.arrivedAt?.let { arrivedAt ->
                Spacer(modifier = Modifier.height(4.dp))
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        imageVector = Icons.Default.Schedule,
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.secondary,
                        modifier = Modifier.size(16.dp)
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = "Arrived: ${arrivedAt.formatShort()}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.secondary
                    )
                }
            }

            Spacer(modifier = Modifier.height(8.dp))
            OutlinedButton(onClick = onNavigateClick) {
                Icon(
                    imageVector = Icons.Default.Map,
                    contentDescription = null,
                    modifier = Modifier.size(16.dp)
                )
                Spacer(modifier = Modifier.width(4.dp))
                Text("Navigate")
            }
        }
    }
}
