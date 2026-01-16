package com.jfleets.driver.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.MenuAnchorType
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.InspectionType
import com.jfleets.driver.ui.components.SignaturePad
import com.jfleets.driver.ui.components.VehicleDiagram
import com.jfleets.driver.viewmodel.ConditionReportViewModel
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ConditionReportScreen(
    loadId: String,
    inspectionType: InspectionType,
    onNavigateBack: () -> Unit,
    onCapturePhoto: () -> Unit,
    onScanVin: () -> Unit,
    viewModel: ConditionReportViewModel = koinViewModel { parametersOf(loadId, inspectionType) }
) {
    val uiState by viewModel.uiState.collectAsState()
    val snackbarHostState = remember { SnackbarHostState() }
    var showDamageDialog by remember { mutableStateOf(false) }
    var pendingDamageX by remember { mutableStateOf(0.0) }
    var pendingDamageY by remember { mutableStateOf(0.0) }

    LaunchedEffect(uiState.error) {
        uiState.error?.let {
            snackbarHostState.showSnackbar(it)
            viewModel.clearError()
        }
    }

    LaunchedEffect(uiState.isSuccess) {
        if (uiState.isSuccess) {
            onNavigateBack()
        }
    }

    val title = when (inspectionType) {
        InspectionType.PICKUP -> "Pickup Inspection"
        InspectionType.DELIVERY -> "Delivery Inspection"
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(title) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                    }
                }
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) }
    ) { padding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            // VIN Input Section
            item {
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
                        value = uiState.vin,
                        onValueChange = { viewModel.setVin(it) },
                        label = { Text("VIN (17 characters)") },
                        placeholder = { Text("Enter or scan VIN") },
                        modifier = Modifier.weight(1f),
                        singleLine = true,
                        trailingIcon = {
                            if (uiState.isDecodingVin) {
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
                    onClick = { viewModel.decodeVin() },
                    enabled = uiState.vin.length == 17 && !uiState.isDecodingVin,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text("Decode VIN")
                }
            }

            // Vehicle Info Display
            if (uiState.vehicleInfo != null) {
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(
                            containerColor = MaterialTheme.colorScheme.secondaryContainer
                        )
                    ) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text(
                                text = "Vehicle Information",
                                style = MaterialTheme.typography.titleSmall
                            )
                            Spacer(modifier = Modifier.height(8.dp))

                            uiState.vehicleInfo?.let { info ->
                                if (info.year != null) {
                                    Text("Year: ${info.year}")
                                }
                                if (!info.make.isNullOrBlank()) {
                                    Text("Make: ${info.make}")
                                }
                                if (!info.model.isNullOrBlank()) {
                                    Text("Model: ${info.model}")
                                }
                                if (!info.bodyClass.isNullOrBlank()) {
                                    Text("Body: ${info.bodyClass}")
                                }
                            }
                        }
                    }
                }
            }

            // Vehicle Diagram
            item {
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
                    damageMarkers = uiState.damageMarkers,
                    onTap = { x, y ->
                        pendingDamageX = x
                        pendingDamageY = y
                        showDamageDialog = true
                    }
                )
            }

            // Damage Markers List
            if (uiState.damageMarkers.isNotEmpty()) {
                item {
                    Text(
                        text = "Marked Damage (${uiState.damageMarkers.size})",
                        style = MaterialTheme.typography.titleSmall
                    )
                }

                itemsIndexed(uiState.damageMarkers) { index, marker ->
                    Card(
                        modifier = Modifier.fillMaxWidth()
                    ) {
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
                                        color = when (marker.severity.lowercase()) {
                                            "severe" -> Color.Red
                                            "moderate" -> Color(0xFFFFA500)
                                            else -> Color.Yellow
                                        }
                                    )
                                }
                            }
                            IconButton(onClick = { viewModel.removeDamageMarker(index) }) {
                                Icon(
                                    Icons.Default.Close,
                                    contentDescription = "Remove",
                                    tint = MaterialTheme.colorScheme.error
                                )
                            }
                        }
                    }
                }
            }

            // Photos Section
            item {
                Text(
                    text = "Photos",
                    style = MaterialTheme.typography.titleMedium
                )

                LazyRow(
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    contentPadding = PaddingValues(vertical = 8.dp)
                ) {
                    item {
                        Card(
                            modifier = Modifier
                                .size(100.dp)
                                .clickable { onCapturePhoto() },
                            colors = CardDefaults.cardColors(
                                containerColor = MaterialTheme.colorScheme.surfaceVariant
                            )
                        ) {
                            Box(
                                modifier = Modifier.fillMaxSize(),
                                contentAlignment = Alignment.Center
                            ) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                    Icon(
                                        imageVector = Icons.Default.Add,
                                        contentDescription = "Add Photo",
                                        modifier = Modifier.size(32.dp)
                                    )
                                    Text(
                                        text = "Add Photo",
                                        style = MaterialTheme.typography.bodySmall,
                                        textAlign = TextAlign.Center
                                    )
                                }
                            }
                        }
                    }

                    items(uiState.photos) { photo ->
                        Box(modifier = Modifier.size(100.dp)) {
                            Card(
                                modifier = Modifier.fillMaxSize(),
                                shape = RoundedCornerShape(8.dp)
                            ) {
                                Box(
                                    modifier = Modifier
                                        .fillMaxSize()
                                        .background(Color.Gray),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Text(
                                        text = photo.fileName,
                                        color = Color.White,
                                        style = MaterialTheme.typography.bodySmall,
                                        textAlign = TextAlign.Center
                                    )
                                }
                            }
                            IconButton(
                                onClick = { viewModel.removePhoto(photo.id) },
                                modifier = Modifier
                                    .align(Alignment.TopEnd)
                                    .size(24.dp)
                                    .background(
                                        color = MaterialTheme.colorScheme.error,
                                        shape = CircleShape
                                    )
                            ) {
                                Icon(
                                    imageVector = Icons.Default.Close,
                                    contentDescription = "Remove",
                                    tint = Color.White,
                                    modifier = Modifier.size(16.dp)
                                )
                            }
                        }
                    }
                }
            }

            // Signature Section
            item {
                SignaturePad(
                    modifier = Modifier.fillMaxWidth(),
                    onSignatureComplete = { paths ->
                        val base64 = "signature_captured"
                        viewModel.setSignature(paths, base64)
                    },
                    onClear = {
                        viewModel.clearSignature()
                    }
                )
            }

            // Notes
            item {
                OutlinedTextField(
                    value = uiState.notes,
                    onValueChange = { viewModel.setNotes(it) },
                    label = { Text("Notes (Optional)") },
                    placeholder = { Text("Any additional notes...") },
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(120.dp),
                    maxLines = 4
                )
            }

            // Submit Button
            item {
                Button(
                    onClick = { viewModel.submit() },
                    enabled = viewModel.canSubmit() && !uiState.isSubmitting,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(56.dp)
                ) {
                    if (uiState.isSubmitting) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(24.dp),
                            color = MaterialTheme.colorScheme.onPrimary
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                    }
                    Text("Submit Condition Report")
                }

                Spacer(modifier = Modifier.height(16.dp))
            }
        }
    }

    // Damage Description Dialog
    if (showDamageDialog) {
        DamageMarkerDialog(
            onDismiss = { showDamageDialog = false },
            onConfirm = { description, severity ->
                viewModel.addDamageMarker(pendingDamageX, pendingDamageY, description, severity)
                showDamageDialog = false
            }
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun DamageMarkerDialog(
    onDismiss: () -> Unit,
    onConfirm: (description: String?, severity: String?) -> Unit
) {
    var description by remember { mutableStateOf("") }
    var severity by remember { mutableStateOf("Minor") }
    var expanded by remember { mutableStateOf(false) }
    val severityOptions = listOf("Minor", "Moderate", "Severe")

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Add Damage Marker") },
        text = {
            Column {
                OutlinedTextField(
                    value = description,
                    onValueChange = { description = it },
                    label = { Text("Description") },
                    placeholder = { Text("e.g., Scratch on door") },
                    modifier = Modifier.fillMaxWidth()
                )

                Spacer(modifier = Modifier.height(16.dp))

                ExposedDropdownMenuBox(
                    expanded = expanded,
                    onExpandedChange = { expanded = !expanded }
                ) {
                    OutlinedTextField(
                        value = severity,
                        onValueChange = {},
                        readOnly = true,
                        label = { Text("Severity") },
                        trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = expanded) },
                        modifier = Modifier
                            .fillMaxWidth()
                            .menuAnchor(MenuAnchorType.PrimaryNotEditable)
                    )

                    ExposedDropdownMenu(
                        expanded = expanded,
                        onDismissRequest = { expanded = false }
                    ) {
                        severityOptions.forEach { option ->
                            DropdownMenuItem(
                                text = { Text(option) },
                                onClick = {
                                    severity = option
                                    expanded = false
                                }
                            )
                        }
                    }
                }
            }
        },
        confirmButton = {
            Button(onClick = {
                onConfirm(
                    description.takeIf { it.isNotBlank() },
                    severity
                )
            }) {
                Text("Add")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    )
}
