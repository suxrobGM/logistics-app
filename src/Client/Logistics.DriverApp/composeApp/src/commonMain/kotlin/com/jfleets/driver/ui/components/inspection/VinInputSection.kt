package com.jfleets.driver.ui.components.inspection

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
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
 * VIN input section with scan and decode functionality.
 */
@Composable
fun VinInputSection(
    vin: String,
    onVinChange: (String) -> Unit,
    onScanVin: () -> Unit,
    onDecodeVin: () -> Unit,
    isDecodingVin: Boolean,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        Text(
            text = "Vehicle VIN",
            style = MaterialTheme.typography.titleMedium
        )

        Spacer(modifier = Modifier.height(8.dp))

        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = vin,
                onValueChange = onVinChange,
                label = { Text("VIN (17 characters)") },
                placeholder = { Text("Enter or scan VIN") },
                modifier = Modifier.weight(1f),
                singleLine = true,
                trailingIcon = {
                    if (isDecodingVin) {
                        CircularProgressIndicator(modifier = Modifier.size(24.dp))
                    }
                }
            )

            Spacer(modifier = Modifier.width(8.dp))

            IconButton(onClick = onScanVin) {
                Icon(Icons.Default.Search, "Scan VIN")
            }
        }

        Spacer(modifier = Modifier.height(8.dp))

        Button(
            onClick = onDecodeVin,
            enabled = vin.length == 17 && !isDecodingVin,
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Decode VIN")
        }
    }
}
