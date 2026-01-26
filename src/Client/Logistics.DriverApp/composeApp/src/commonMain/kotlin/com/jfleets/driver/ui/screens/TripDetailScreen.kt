package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.LocalShipping
import androidx.compose.material.icons.filled.Map
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.TripDto
import com.jfleets.driver.api.models.TripLoadDto
import com.jfleets.driver.api.models.TripStopDto
import com.jfleets.driver.model.DistanceUnit
import com.jfleets.driver.model.LocalUserSettings
import com.jfleets.driver.model.toDisplayString
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.CardContainer
import com.jfleets.driver.ui.components.DetailRow
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.ui.components.SectionCard
import com.jfleets.driver.ui.components.TripStatusChip
import com.jfleets.driver.ui.components.TripStopItem
import com.jfleets.driver.util.formatCurrency
import com.jfleets.driver.util.formatDistance
import com.jfleets.driver.util.formatShort
import com.jfleets.driver.viewmodel.TripDetailUiState
import com.jfleets.driver.viewmodel.TripDetailViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TripDetailScreen(
    onNavigateBack: () -> Unit,
    onOpenMaps: (String) -> Unit,
    onLoadClick: (String) -> Unit,
    viewModel: TripDetailViewModel
) {
    val userSettings = LocalUserSettings.current
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Trip Details",
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                }
            )
        }
    ) { paddingValues ->
        when (val state = uiState) {
            is TripDetailUiState.Loading -> LoadingIndicator()

            is TripDetailUiState.Success -> {
                val trip = state.trip
                val stops = trip.stops?.sortedBy { it.order } ?: emptyList()

                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues)
                        .verticalScroll(rememberScrollState())
                        .padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    TripHeaderCard(trip = trip)

                    TripRouteCard(
                        trip = trip,
                        stopsCount = stops.size,
                        distanceUnit = userSettings.distanceUnit
                    )

                    // View Route on Maps Button
                    Button(
                        onClick = { onOpenMaps(viewModel.getGoogleMapsUrl(trip)) },
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Icon(Icons.Default.Map, "Map")
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("View Full Route on Maps")
                    }

                    if (stops.isNotEmpty()) {
                        TripStopsCard(
                            stops = stops,
                            onNavigateToStop = { stop ->
                                onOpenMaps(viewModel.getStopGoogleMapsUrl(stop))
                            }
                        )
                    }

                    trip.loads?.takeIf { it.isNotEmpty() }?.let { loads ->
                        TripLoadsCard(
                            loads = loads,
                            onLoadClick = onLoadClick
                        )
                    }

                    TripTimelineCard(trip = trip)
                }
            }

            is TripDetailUiState.Error -> {
                ErrorView(
                    message = state.message,
                    onRetry = { viewModel.refresh() }
                )
            }
        }
    }
}

@Composable
private fun TripHeaderCard(trip: TripDto) {
    CardContainer {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Column {
                    Text(
                        text = trip.name ?: "Trip #${trip.number}",
                        style = MaterialTheme.typography.headlineSmall,
                        fontWeight = FontWeight.Bold
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                    Text(
                        text = "Trip #${trip.number}",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                TripStatusChip(status = trip.status)
            }

            trip.totalRevenue?.let { revenue ->
                Spacer(modifier = Modifier.height(12.dp))
                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
                Spacer(modifier = Modifier.height(12.dp))
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Total Revenue",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Text(
                        text = revenue.formatCurrency(),
                        style = MaterialTheme.typography.titleLarge,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.primary
                    )
                }
            }
        }
    }
}

@Composable
private fun TripRouteCard(
    trip: TripDto,
    stopsCount: Int,
    distanceUnit: DistanceUnit
) {
    SectionCard(title = "Route") {
        DetailRow("Origin", trip.originAddress.toDisplayString())
        Spacer(modifier = Modifier.height(8.dp))
        DetailRow("Destination", trip.destinationAddress.toDisplayString())
        Spacer(modifier = Modifier.height(12.dp))
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            trip.totalDistance?.let {
                DetailRow("Distance", it.formatDistance(distanceUnit))
            }
            DetailRow("Stops", "$stopsCount")
        }
    }
}

@Composable
private fun TripStopsCard(
    stops: List<TripStopDto>,
    onNavigateToStop: (TripStopDto) -> Unit
) {
    SectionCard(title = "Stops") {
        stops.forEachIndexed { index, stop ->
            TripStopItem(
                stop = stop,
                stopNumber = index + 1,
                onNavigateClick = { onNavigateToStop(stop) }
            )
            if (index < stops.size - 1) {
                HorizontalDivider(
                    modifier = Modifier.padding(vertical = 12.dp),
                    color = MaterialTheme.colorScheme.outlineVariant
                )
            }
        }
    }
}

@Composable
private fun TripLoadsCard(
    loads: List<TripLoadDto>,
    onLoadClick: (String) -> Unit
) {
    SectionCard(title = "Loads (${loads.size})") {
        loads.forEach { load ->
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 8.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Icon(
                    imageVector = Icons.Default.LocalShipping,
                    contentDescription = "Load",
                    tint = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.size(24.dp)
                )
                Spacer(modifier = Modifier.width(12.dp))
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = load.name ?: "Load #${load.number}",
                        style = MaterialTheme.typography.bodyLarge,
                        fontWeight = FontWeight.Medium
                    )
                    load.customer.name?.let {
                        Text(
                            text = it,
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
                load.id?.let { loadId ->
                    OutlinedButton(onClick = { onLoadClick(loadId) }) {
                        Text("View")
                    }
                }
            }
        }
    }
}

@Composable
private fun TripTimelineCard(trip: TripDto) {
    SectionCard(title = "Timeline") {
        trip.createdAt?.let {
            DetailRow("Created", it.formatShort())
        }
        trip.dispatchedAt?.let {
            Spacer(modifier = Modifier.height(8.dp))
            DetailRow("Dispatched", it.formatShort())
        }
        trip.completedAt?.let {
            Spacer(modifier = Modifier.height(8.dp))
            DetailRow("Completed", it.formatShort())
        }
        trip.cancelledAt?.let {
            Spacer(modifier = Modifier.height(8.dp))
            DetailRow("Cancelled", it.formatShort())
        }
    }
}
