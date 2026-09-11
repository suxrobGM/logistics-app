package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.logisticsx.driver.api.models.LoadStatus
import com.logisticsx.driver.model.LocalUserSettings
import com.logisticsx.driver.model.getMapsUrl
import com.logisticsx.driver.model.toDisplayString
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.CardContainer
import com.logisticsx.driver.ui.components.DetailRow
import com.logisticsx.driver.ui.components.UiStateContent
import com.logisticsx.driver.ui.icons.AppIcons
import com.logisticsx.driver.util.formatCurrency
import com.logisticsx.driver.util.formatDistance
import com.logisticsx.driver.util.formatShort
import com.logisticsx.driver.viewmodel.LoadDetailViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoadDetailScreen(
    onNavigateBack: () -> Unit,
    onOpenMaps: (String) -> Unit,
    onCapturePod: (loadId: String) -> Unit = {},
    onCaptureBol: (loadId: String) -> Unit = {},
    onPickupInspection: (loadId: String) -> Unit = {},
    onDeliveryInspection: (loadId: String) -> Unit = {},
    viewModel: LoadDetailViewModel
) {
    val userSettings = LocalUserSettings.current
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Load Details",
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(AppIcons.ArrowBack, "Back")
                    }
                }
            )
        }
    ) { paddingValues ->
        UiStateContent(uiState, viewModel::refresh) { load ->
            Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues)
                        .verticalScroll(rememberScrollState())
                        .padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    CardContainer {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text(
                                text = load.name ?: "",
                                style = MaterialTheme.typography.headlineSmall,
                                fontWeight = FontWeight.Bold
                            )
                            Spacer(modifier = Modifier.height(4.dp))
                            Text(
                                text = "Load #${load.number}",
                                style = MaterialTheme.typography.bodyMedium,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                        }
                    }

                    CardContainer {
                        Column(modifier = Modifier.padding(16.dp)) {
                            DetailRow("Origin", load.originAddress.toDisplayString())
                            load.originTerminalCode?.let {
                                Spacer(modifier = Modifier.height(4.dp))
                                DetailRow(
                                    "Origin Terminal",
                                    "$it - ${load.originTerminalName ?: ""}"
                                )
                            }
                            Spacer(modifier = Modifier.height(8.dp))
                            DetailRow("Destination", load.destinationAddress.toDisplayString())
                            load.destinationTerminalCode?.let {
                                Spacer(modifier = Modifier.height(4.dp))
                                DetailRow(
                                    "Destination Terminal",
                                    "$it - ${load.destinationTerminalName ?: ""}"
                                )
                            }
                            Spacer(modifier = Modifier.height(12.dp))
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween
                            ) {
                                DetailRow("Cost", load.deliveryCost?.formatCurrency() ?: "")
                                DetailRow("Distance", load.distance?.formatDistance(userSettings.distanceUnit) ?: "")
                            }
                        }
                    }

                    if (load.containerNumber != null) {
                        CardContainer {
                            Column(modifier = Modifier.padding(16.dp)) {
                                Text(
                                    text = "Container",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Bold
                                )
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Number", load.containerNumber)
                                load.containerIsoType?.let {
                                    Spacer(modifier = Modifier.height(4.dp))
                                    DetailRow("ISO Type", it.name.replace("_", " "))
                                }
                            }
                        }
                    }

                    if (load.requestedPickupDate != null || load.requestedDeliveryDate != null) {
                        CardContainer {
                            Column(modifier = Modifier.padding(16.dp)) {
                                Text(
                                    text = "Schedule",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Bold
                                )
                                Spacer(modifier = Modifier.height(8.dp))
                                load.requestedPickupDate?.let {
                                    DetailRow("Requested Pickup", it.formatShort())
                                    Spacer(modifier = Modifier.height(4.dp))
                                }
                                load.requestedDeliveryDate?.let {
                                    DetailRow("Requested Delivery", it.formatShort())
                                }
                            }
                        }
                    }

                    CardContainer {
                        Column(modifier = Modifier.padding(16.dp)) {
                            DetailRow("Status", load.status?.name?.replace("_", " ") ?: "Unknown")
                            load.assignedDispatcherName?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Dispatcher", it)
                            }
                            load.createdAt?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Created", it.formatShort())
                            }
                            load.pickedUpAt?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Picked Up", it.formatShort())
                            }
                            load.deliveredAt?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Delivered", it.formatShort())
                            }
                        }
                    }

                    // A load always carries coordinates, so this button needs no null check.
                    Button(
                        onClick = { onOpenMaps(load.getMapsUrl()) },
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Icon(AppIcons.Map, "Map")
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("View Route on Maps")
                    }

                    when (load.status) {
                        LoadStatus.DISPATCHED -> {
                            OutlinedButton(
                                onClick = { load.id?.let { onCaptureBol(it) } },
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Icon(AppIcons.Description, "BOL")
                                Spacer(modifier = Modifier.width(8.dp))
                                Text("Capture Bill of Lading")
                            }

                            Spacer(modifier = Modifier.height(8.dp))

                            Button(
                                onClick = { viewModel.confirmPickup() },
                                modifier = Modifier.fillMaxWidth(),
                                enabled = load.canConfirmPickUp == true
                            ) {
                                Text("Confirm Pick Up")
                            }
                        }

                        LoadStatus.PICKED_UP -> {
                            OutlinedButton(
                                onClick = { load.id?.let { onCapturePod(it) } },
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Icon(AppIcons.CameraAlt, "POD")
                                Spacer(modifier = Modifier.width(8.dp))
                                Text("Capture Proof of Delivery")
                            }

                            Spacer(modifier = Modifier.height(8.dp))

                            Button(
                                onClick = { viewModel.confirmDelivery() },
                                modifier = Modifier.fillMaxWidth(),
                                enabled = load.canConfirmDelivery == true
                            ) {
                                Text("Confirm Delivery")
                            }
                        }

                        else -> {}
                    }

                    if (load.status in listOf(
                            LoadStatus.DISPATCHED,
                            LoadStatus.PICKED_UP,
                            LoadStatus.DELIVERED
                        )
                    ) {
                        CardContainer {
                            Column(modifier = Modifier.padding(16.dp)) {
                                Text(
                                    text = "Vehicle Condition Reports",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.Bold
                                )
                                Spacer(modifier = Modifier.height(12.dp))

                                OutlinedButton(
                                    onClick = { load.id?.let { onPickupInspection(it) } },
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Icon(AppIcons.Description, "Pickup Inspection")
                                    Spacer(modifier = Modifier.width(8.dp))
                                    Text("Pickup Inspection")
                                }

                                Spacer(modifier = Modifier.height(8.dp))

                                OutlinedButton(
                                    onClick = { load.id?.let { onDeliveryInspection(it) } },
                                    modifier = Modifier.fillMaxWidth()
                                ) {
                                    Icon(AppIcons.Description, "Delivery Inspection")
                                    Spacer(modifier = Modifier.width(8.dp))
                                    Text("Delivery Inspection")
                                }
                            }
                        }
                    }
                }
        }
    }
}
