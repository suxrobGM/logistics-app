package com.jfleets.driver.ui.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Route
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.TripDto
import com.jfleets.driver.api.models.TripStatus
import com.jfleets.driver.model.LocalUserSettings
import com.jfleets.driver.model.toDisplayString
import com.jfleets.driver.util.formatDistance

@Composable
fun TripCard(
    trip: TripDto,
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    val userSettings = LocalUserSettings.current

    CardContainer(
        modifier = modifier
            .fillMaxWidth()
            .clickable(onClick = onClick)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                imageVector = Icons.Default.Route,
                contentDescription = "Trip",
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(48.dp)
            )

            Spacer(modifier = Modifier.width(16.dp))

            Column(
                modifier = Modifier.weight(1f)
            ) {
                Text(
                    text = trip.name ?: "Trip #${trip.number}",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.onSurface
                )
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "${trip.originAddress.toDisplayString()} → ${trip.destinationAddress.toDisplayString()}",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    maxLines = 2
                )
                Spacer(modifier = Modifier.height(8.dp))
                Row(
                    horizontalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    TripStatusChip(status = trip.status)
                    trip.totalDistance?.let { distance ->
                        Chip(
                            text = distance.formatDistance(userSettings.distanceUnit),
                            color = MaterialTheme.colorScheme.secondary
                        )
                    }
                    trip.stops?.size?.let { stopsCount ->
                        Chip(
                            text = "$stopsCount stops",
                            color = MaterialTheme.colorScheme.tertiary
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun TripStatusChip(status: TripStatus?) {
    val (text, color) = when (status) {
        TripStatus.DRAFT -> "Draft" to MaterialTheme.colorScheme.outline
        TripStatus.DISPATCHED -> "Dispatched" to MaterialTheme.colorScheme.primary
        TripStatus.IN_TRANSIT -> "In Transit" to MaterialTheme.colorScheme.tertiary
        TripStatus.COMPLETED -> "Completed" to MaterialTheme.colorScheme.secondary
        TripStatus.CANCELLED -> "Cancelled" to MaterialTheme.colorScheme.error
        null -> "Unknown" to MaterialTheme.colorScheme.outline
    }
    Chip(text = text, color = color)
}
