package com.jfleets.driver.presentation.ui.screens.pastloads

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.jfleets.driver.presentation.ui.components.ErrorView
import com.jfleets.driver.presentation.ui.components.LoadCard
import com.jfleets.driver.presentation.ui.components.LoadingIndicator
import com.jfleets.driver.presentation.viewmodel.PastLoadsUiState
import com.jfleets.driver.presentation.viewmodel.PastLoadsViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PastLoadsScreen(
    onLoadClick: (Double) -> Unit,
    viewModel: PastLoadsViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Past Loads") },
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                }
            )
        }
    ) { paddingValues ->
        when (val state = uiState) {
            is PastLoadsUiState.Loading -> {
                LoadingIndicator()
            }
            is PastLoadsUiState.Success -> {
                if (state.loads.isEmpty()) {
                    Box(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(paddingValues)
                    ) {
                        Text(
                            text = "No past loads found",
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
                        items(state.loads) { load ->
                            LoadCard(
                                load = load,
                                onClick = { onLoadClick(load.id) }
                            )
                        }
                    }
                }
            }
            is PastLoadsUiState.Error -> {
                ErrorView(
                    message = state.message,
                    onRetry = { viewModel.refresh() }
                )
            }
        }
    }
}
