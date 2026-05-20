package com.logisticsx.driver.ui.components.inspection

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/**
 * Identifier section shown when inspecting container cargo (intermodal /
 * tank / reefer). Captures the ISO 6346 container number and the seal
 * number recorded at pickup or delivery.
 *
 * Mirrors the structure of [VinInputSection] for visual consistency.
 */
@Composable
fun ContainerInputSection(
    containerNumber: String,
    sealNumber: String,
    onContainerNumberChange: (String) -> Unit,
    onSealNumberChange: (String) -> Unit,
    onScanContainer: () -> Unit,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        Text(
            text = "Container",
            style = MaterialTheme.typography.titleMedium
        )

        Spacer(modifier = Modifier.height(8.dp))

        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = containerNumber,
                onValueChange = { onContainerNumberChange(it.uppercase()) },
                label = { Text("Container Number (ISO 6346)") },
                placeholder = { Text("e.g. MSCU1234567") },
                modifier = Modifier.weight(1f),
                singleLine = true
            )

            Spacer(modifier = Modifier.width(8.dp))

            IconButton(onClick = onScanContainer) {
                Icon(Icons.Default.Search, "Scan container number")
            }
        }

        Spacer(modifier = Modifier.height(8.dp))

        OutlinedTextField(
            value = sealNumber,
            onValueChange = onSealNumberChange,
            label = { Text("Seal Number") },
            placeholder = { Text("Seal number on container doors") },
            modifier = Modifier.fillMaxWidth(),
            singleLine = true
        )
    }
}
