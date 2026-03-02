package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.LocalShipping
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.TripDto
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.EmptyStateView
import com.logisticsx.driver.ui.components.ErrorView
import com.logisticsx.driver.ui.components.LoadingIndicator
import com.logisticsx.driver.ui.components.TripCard
import com.logisticsx.driver.viewmodel.TripsViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TripsScreen(
    onTripClick: (String) -> Unit,
    viewModel: TripsViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Trips",
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                }
            )
        }
    ) { paddingValues ->
        when (uiState) {
            is UiState.Loading -> {
                LoadingIndicator()
            }

            is UiState.Error -> {
                ErrorView(
                    message = (uiState as UiState.Error).message,
                    onRetry = { viewModel.refresh() }
                )
            }

            is UiState.Success<*> -> {
                @Suppress("UNCHECKED_CAST")
                val trips = (uiState as UiState.Success<List<TripDto>>).data
                if (trips.isEmpty()) {
                    EmptyStateView(
                        icon = Icons.Default.LocalShipping,
                        title = "No trips found",
                        message = "Pull to refresh or check back later",
                        modifier = Modifier.padding(paddingValues)
                    )
                } else {
                    LazyColumn(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(paddingValues),
                        contentPadding = PaddingValues(16.dp),
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        items(trips) { trip ->
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
}
