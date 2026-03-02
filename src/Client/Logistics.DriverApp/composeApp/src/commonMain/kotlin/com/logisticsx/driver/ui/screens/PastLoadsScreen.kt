package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.History
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
import com.logisticsx.driver.api.models.LoadDto
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.EmptyStateView
import com.logisticsx.driver.ui.components.ErrorView
import com.logisticsx.driver.ui.components.LoadCard
import com.logisticsx.driver.ui.components.LoadingIndicator
import com.logisticsx.driver.viewmodel.PastLoadsViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PastLoadsScreen(
    onLoadClick: (String) -> Unit,
    viewModel: PastLoadsViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Past Loads",
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                }
            )
        }
    ) { paddingValues ->
        when (val state = uiState) {
            is UiState.Loading -> {
                LoadingIndicator()
            }

            is UiState.Success<*> -> {
                @Suppress("UNCHECKED_CAST")
                val loads = (state as UiState.Success<List<LoadDto>>).data
                if (loads.isEmpty()) {
                    EmptyStateView(
                        icon = Icons.Default.History,
                        title = "No past loads found",
                        message = "Completed loads will appear here",
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
                        items(loads) { load ->
                            LoadCard(
                                load = load,
                                onClick = { onLoadClick(load.id!!) }
                            )
                        }
                    }
                }
            }

            is UiState.Error -> {
                ErrorView(
                    message = state.message,
                    onRetry = { viewModel.refresh() }
                )
            }
        }
    }
}
