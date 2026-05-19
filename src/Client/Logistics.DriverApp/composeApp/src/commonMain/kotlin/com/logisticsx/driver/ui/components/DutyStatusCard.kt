package com.logisticsx.driver.ui.components

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/**
 * Dashboard card showing whether the driver is On Duty (location is being
 * shared with dispatch) or Off Duty. Tapping the switch flips the state via
 * [onToggle], which is wired to `DutyStatusManager` in the ViewModel.
 */
@Composable
fun DutyStatusCard(
    isOnDuty: Boolean,
    onToggle: (goingOnDuty: Boolean) -> Unit,
    modifier: Modifier = Modifier
) {
    CardContainer(modifier = modifier) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Chip(
                    text = if (isOnDuty) "On Duty" else "Off Duty",
                    color = if (isOnDuty) {
                        MaterialTheme.colorScheme.primary
                    } else {
                        MaterialTheme.colorScheme.onSurfaceVariant
                    }
                )
                Spacer(modifier = Modifier.height(8.dp))
                Text(
                    text = if (isOnDuty) {
                        "Sharing truck location with dispatch."
                    } else {
                        "Location sharing is off. Tap to start a shift."
                    },
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
            Switch(
                checked = isOnDuty,
                onCheckedChange = { onToggle(it) }
            )
        }
    }
}
