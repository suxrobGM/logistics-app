package com.jfleets.driver.ui.screens.dvir

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.MenuAnchorType
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.TruckDto
import com.jfleets.driver.ui.components.SectionCard

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DvirTruckSelector(
    selectedTruck: TruckDto?,
    availableTrucks: List<TruckDto>,
    isLoading: Boolean,
    onTruckSelected: (TruckDto) -> Unit,
    modifier: Modifier = Modifier
) {
    var expanded by remember { mutableStateOf(false) }

    SectionCard(
        title = "Select Truck",
        modifier = modifier
    ) {
        if (isLoading) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(56.dp),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator(modifier = Modifier.size(24.dp))
            }
        } else {
            ExposedDropdownMenuBox(
                expanded = expanded,
                onExpandedChange = { expanded = it }
            ) {
                OutlinedTextField(
                    value = selectedTruck?.number ?: "",
                    onValueChange = {},
                    readOnly = true,
                    label = { Text("Truck") },
                    placeholder = { Text("Select a truck") },
                    trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = expanded) },
                    modifier = Modifier
                        .fillMaxWidth()
                        .menuAnchor(MenuAnchorType.PrimaryNotEditable)
                )

                ExposedDropdownMenu(
                    expanded = expanded,
                    onDismissRequest = { expanded = false }
                ) {
                    availableTrucks.forEach { truck ->
                        DropdownMenuItem(
                            text = { Text(truck.number ?: "") },
                            onClick = {
                                onTruckSelected(truck)
                                expanded = false
                            }
                        )
                    }
                }
            }
        }
    }
}
