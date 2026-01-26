package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.pulltorefresh.PullToRefreshBox
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.LoadDto
import com.jfleets.driver.api.models.TripDto
import com.jfleets.driver.model.driversList
import com.jfleets.driver.model.fullName
import com.jfleets.driver.permission.RequestBackgroundLocationIfNeeded
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.CardContainer
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadCard
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.ui.components.TripCard
import com.jfleets.driver.viewmodel.DashboardUiState
import com.jfleets.driver.viewmodel.DashboardViewModel
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    onLoadClick: (String) -> Unit,
    onTripClick: (String) -> Unit,
    onLogout: () -> Unit,
    viewModel: DashboardViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val isRefreshing = uiState is DashboardUiState.Loading

    // Request background location permission (only if foreground location already granted)
    RequestBackgroundLocationIfNeeded()

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Dashboard",
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                    IconButton(onClick = {
                        viewModel.logout()
                        onLogout()
                    }) {
                        Icon(Icons.AutoMirrored.Filled.ExitToApp, "Logout")
                    }
                }
            )
        }
    ) { paddingValues ->
        PullToRefreshBox(
            isRefreshing = isRefreshing,
            onRefresh = { viewModel.refresh() },
            modifier = Modifier.padding(paddingValues)
        ) {
            when (val state = uiState) {
                is DashboardUiState.Loading -> {
                    LoadingIndicator()
                }

                is DashboardUiState.Success -> {
                    val truck = state.truck
                    LazyColumn(
                        modifier = Modifier.fillMaxSize(),
                        contentPadding = PaddingValues(16.dp),
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        // Truck Info Card
                        item {
                            CardContainer {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Text(
                                        text = "Truck #${truck.number}",
                                        style = MaterialTheme.typography.titleLarge,
                                        fontWeight = FontWeight.Bold
                                    )
                                    Spacer(modifier = Modifier.height(8.dp))
                                    for (driver in truck.driversList) {
                                        Text(
                                            text = "Driver: ${driver.fullName()}",
                                            style = MaterialTheme.typography.bodyMedium,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                }
                            }
                        }

                        // Active Loads Section
                        item {
                            Text(
                                text = "Active Loads",
                                style = MaterialTheme.typography.titleLarge,
                                fontWeight = FontWeight.Bold,
                                modifier = Modifier.padding(vertical = 8.dp)
                            )
                        }

                        if (truck.loads == null || truck.loads.isEmpty()) {
                            item {
                                CardContainer {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(32.dp)
                                    ) {
                                        Text(
                                            text = "No active loads",
                                            style = MaterialTheme.typography.bodyLarge,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                }
                            }
                        } else {
                            items(truck.loads) { load: LoadDto ->
                                LoadCard(
                                    load = load,
                                    onClick = { onLoadClick(load.id!!) }
                                )
                            }
                        }

                        // Active Trips Section
                        item {
                            Text(
                                text = "Active Trips",
                                style = MaterialTheme.typography.titleLarge,
                                fontWeight = FontWeight.Bold,
                                modifier = Modifier.padding(vertical = 8.dp)
                            )
                        }

                        if (state.trips.isEmpty()) {
                            item {
                                CardContainer {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(32.dp)
                                    ) {
                                        Text(
                                            text = "No active trips",
                                            style = MaterialTheme.typography.bodyLarge,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                }
                            }
                        } else {
                            items(state.trips) { trip: TripDto ->
                                TripCard(
                                    trip = trip,
                                    onClick = { onTripClick(trip.id!!) }
                                )
                            }
                        }
                    }
                }

                is DashboardUiState.Error -> {
                    ErrorView(
                        message = state.message,
                        onRetry = { viewModel.refresh() }
                    )
                }
            }
        }
    }
}
