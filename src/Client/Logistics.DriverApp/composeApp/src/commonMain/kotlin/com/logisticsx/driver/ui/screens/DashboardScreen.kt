package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Assignment
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material.icons.filled.Assignment
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.pulltorefresh.PullToRefreshBox
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.LoadDto
import com.logisticsx.driver.api.models.TripDto
import com.logisticsx.driver.model.driversList
import com.logisticsx.driver.model.fullName
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.CardContainer
import com.logisticsx.driver.ui.components.DutyStatusCard
import com.logisticsx.driver.ui.components.LoadCard
import com.logisticsx.driver.ui.components.TripCard
import com.logisticsx.driver.ui.components.UiStateContent
import com.logisticsx.driver.viewmodel.DashboardViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    onLoadClick: (String) -> Unit,
    onTripClick: (String) -> Unit,
    onDvirClick: (truckId: String?) -> Unit,
    onLogout: () -> Unit,
    viewModel: DashboardViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val isRefreshing = uiState is UiState.Loading
    val isOnDuty by viewModel.isOnDuty.collectAsState()
    var showLogoutDialog by remember { mutableStateOf(false) }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Dashboard",
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                    IconButton(onClick = { showLogoutDialog = true }) {
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
            UiStateContent(uiState, viewModel::refresh) { dashboardData ->
                val truck = dashboardData.truck
                val trips = dashboardData.trips
                LazyColumn(
                        modifier = Modifier.fillMaxSize(),
                        contentPadding = PaddingValues(16.dp),
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        // Duty Status Card
                        item {
                            DutyStatusCard(
                                isOnDuty = isOnDuty,
                                onToggle = { goingOnDuty ->
                                    if (goingOnDuty) viewModel.goOnDuty() else viewModel.goOffDuty()
                                }
                            )
                        }

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
                                    Spacer(modifier = Modifier.height(16.dp))
                                    Button(
                                        onClick = { onDvirClick(truck.id) },
                                        modifier = Modifier.fillMaxWidth()
                                    ) {
                                        Icon(
                                            imageVector = Icons.AutoMirrored.Filled.Assignment,
                                            contentDescription = null
                                        )
                                        Spacer(modifier = Modifier.width(8.dp))
                                        Text("Truck DVIR (Pre-Trip)")
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
                                    Text(
                                        text = "No active loads",
                                        style = MaterialTheme.typography.bodyLarge,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(32.dp)
                                    )
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

                        if (trips.isEmpty()) {
                            item {
                                CardContainer {
                                    Text(
                                        text = "No active trips",
                                        style = MaterialTheme.typography.bodyLarge,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(32.dp)
                                    )
                                }
                            }
                        } else {
                            items(trips) { trip: TripDto ->
                                TripCard(
                                    trip = trip,
                                    onClick = { onTripClick(trip.id!!) }
                                )
                            }
                        }
                    }
            }
        }
    }

    if (showLogoutDialog) {
        AlertDialog(
            onDismissRequest = { showLogoutDialog = false },
            title = { Text("Log out?") },
            text = {
                Text(
                    "You'll need to sign in again to access your trips and loads. " +
                        "Location sharing will also stop."
                )
            },
            confirmButton = {
                Button(onClick = {
                    showLogoutDialog = false
                    viewModel.logout()
                    onLogout()
                }) {
                    Text("Log out")
                }
            },
            dismissButton = {
                TextButton(onClick = { showLogoutDialog = false }) {
                    Text("Cancel")
                }
            }
        )
    }
}
