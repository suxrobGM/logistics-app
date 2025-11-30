@file:OptIn(ExperimentalTime::class)

package com.jfleets.driver.ui.screens

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
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Map
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.model.LoadStatus
import com.jfleets.driver.ui.components.CardContainer
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.util.formatCurrency
import com.jfleets.driver.util.formatDistance
import com.jfleets.driver.util.formatShort
import com.jfleets.driver.viewmodel.LoadDetailUiState
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import kotlin.time.ExperimentalTime

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoadDetailScreen(
    onNavigateBack: () -> Unit,
    onOpenMaps: (String) -> Unit,
    viewModel: LoadDetailViewModel
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Load Details") },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                }
            )
        }
    ) { paddingValues ->
        when (val state = uiState) {
            is LoadDetailUiState.Loading -> {
                LoadingIndicator()
            }

            is LoadDetailUiState.Success -> {
                val load = state.load
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues)
                        .verticalScroll(rememberScrollState())
                        .padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    // Load Header
                    CardContainer {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text(
                                text = load.name,
                                style = MaterialTheme.typography.headlineSmall,
                                fontWeight = FontWeight.Bold
                            )
                            Spacer(modifier = Modifier.height(4.dp))
                            Text(
                                text = "Load #${load.refId ?: load.id}",
                                style = MaterialTheme.typography.bodyMedium,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                        }
                    }

                    // Route Information
                    CardContainer {
                        Column(modifier = Modifier.padding(16.dp)) {
                            DetailRow("Origin", load.sourceAddress)
                            Spacer(modifier = Modifier.height(8.dp))
                            DetailRow("Destination", load.destinationAddress)
                            Spacer(modifier = Modifier.height(12.dp))
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween
                            ) {
                                DetailRow("Cost", load.deliveryCost.formatCurrency())
                                DetailRow("Distance", load.distance.formatDistance())
                            }
                        }
                    }

                    // Load Details
                    CardContainer {
                        Column(modifier = Modifier.padding(16.dp)) {
                            DetailRow("Status", load.status.name.replace("_", " "))
                            load.assignedDispatcherName?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Dispatcher", it)
                            }
                            load.createdDate?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Created", it.formatShort())
                            }
                            load.pickUpDate?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Picked Up", it.formatShort())
                            }
                            load.deliveryDate?.let {
                                Spacer(modifier = Modifier.height(8.dp))
                                DetailRow("Delivered", it.formatShort())
                            }
                        }
                    }

                    // Map Button
                    if (load.originLatitude != null && load.originLongitude != null &&
                        load.destinationLatitude != null && load.destinationLongitude != null
                    ) {
                        Button(
                            onClick = {
                                val mapsUrl = viewModel.getGoogleMapsUrl(load)
                                onOpenMaps(mapsUrl)
                            },
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Icon(Icons.Default.Map, "Map")
                            Spacer(modifier = Modifier.width(8.dp))
                            Text("View Route on Maps")
                        }
                    }

                    // Action Buttons
                    when (load.status) {
                        LoadStatus.DISPATCHED -> {
                            Button(
                                onClick = { viewModel.confirmPickup() },
                                modifier = Modifier.fillMaxWidth(),
                                enabled = load.canConfirmPickup
                            ) {
                                Text("Confirm Pick Up")
                            }
                        }

                        LoadStatus.PICKED_UP -> {
                            Button(
                                onClick = { viewModel.confirmDelivery() },
                                modifier = Modifier.fillMaxWidth(),
                                enabled = load.canConfirmDelivery
                            ) {
                                Text("Confirm Delivery")
                            }
                        }

                        else -> {}
                    }
                }
            }

            is LoadDetailUiState.Error -> {
                ErrorView(
                    message = state.message,
                    onRetry = { viewModel.refresh() }
                )
            }
        }
    }
}

@Composable
private fun DetailRow(label: String, value: String) {
    Column {
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodyLarge,
            fontWeight = FontWeight.Medium
        )
    }
}
