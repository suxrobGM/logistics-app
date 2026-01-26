package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.ui.components.TripCard
import com.jfleets.driver.viewmodel.TripsUiState
import com.jfleets.driver.viewmodel.TripsViewModel
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
        when (val state = uiState) {
            is TripsUiState.Loading -> {
                LoadingIndicator()
            }

            is TripsUiState.Success -> {
                if (state.trips.isEmpty()) {
                    Box(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(paddingValues)
                    ) {
                        Text(
                            text = "No trips found",
                            style = MaterialTheme.typography.bodyLarge,
                            modifier = Modifier.padding(16.dp)
                        )
                    }
                } else {
                    LazyColumn(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(paddingValues),
                        contentPadding = PaddingValues(16.dp),
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        items(state.trips) { trip ->
                            TripCard(
                                trip = trip,
                                onClick = { onTripClick(trip.id!!) }
                            )
                        }
                    }
                }
            }

            is TripsUiState.Error -> {
                ErrorView(
                    message = state.message,
                    onRetry = { viewModel.refresh() }
                )
            }
        }
    }
}
