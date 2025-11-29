package com.jfleets.driver.presentation.ui.screens

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
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.presentation.ui.components.CardContainer
import com.jfleets.driver.presentation.ui.components.ErrorView
import com.jfleets.driver.presentation.ui.components.LoadingIndicator
import com.jfleets.driver.presentation.viewmodel.ChartUiState
import com.jfleets.driver.presentation.viewmodel.StatsUiState
import com.jfleets.driver.presentation.viewmodel.StatsViewModel
import com.jfleets.driver.util.formatCurrency
import com.jfleets.driver.util.formatDistance
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun StatsScreen(
    viewModel: StatsViewModel = koinViewModel()
) {
    val statsState by viewModel.statsState.collectAsState()
    val chartState by viewModel.chartState.collectAsState()
    val selectedRange by viewModel.selectedRange.collectAsState()
    val dateRanges = remember { viewModel.getDateRanges() }
    var showRangePicker by remember { mutableStateOf(false) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("My Stats") },
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                }
            )
        }
    ) { paddingValues ->
        when (val state = statsState) {
            is StatsUiState.Loading -> {
                LoadingIndicator()
            }

            is StatsUiState.Success -> {
                val stats = state.stats
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    // Weekly Stats Card
                    item {
                        Text(
                            text = "Weekly Stats",
                            style = MaterialTheme.typography.titleLarge,
                            fontWeight = FontWeight.Bold
                        )
                    }
                    item {
                        CardContainer {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(16.dp),
                                horizontalArrangement = Arrangement.SpaceEvenly
                            ) {
                                StatItem("Gross", stats.weeklyGross.formatCurrency())
                                StatItem("Income", stats.weeklyIncome.formatCurrency())
                                StatItem("Distance", stats.weeklyDistance.formatDistance())
                            }
                        }
                    }

                    // Monthly Stats Card
                    item {
                        Text(
                            text = "Monthly Stats",
                            style = MaterialTheme.typography.titleLarge,
                            fontWeight = FontWeight.Bold
                        )
                    }
                    item {
                        CardContainer {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(16.dp),
                                horizontalArrangement = Arrangement.SpaceEvenly
                            ) {
                                StatItem("Gross", stats.monthlyGross.formatCurrency())
                                StatItem("Income", stats.monthlyIncome.formatCurrency())
                                StatItem("Distance", stats.monthlyDistance.formatDistance())
                            }
                        }
                    }

                    // Chart Section
                    item {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text(
                                text = "Performance Chart",
                                style = MaterialTheme.typography.titleLarge,
                                fontWeight = FontWeight.Bold
                            )
                            TextButton(onClick = { showRangePicker = true }) {
                                Text(selectedRange.name)
                            }
                        }
                    }

                    item {
                        CardContainer {
                            when (val chart = chartState) {
                                is ChartUiState.Loading -> {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .height(250.dp)
                                    ) {
                                        LoadingIndicator()
                                    }
                                }

                                is ChartUiState.Success -> {
                                    Column(modifier = Modifier.padding(16.dp)) {
                                        Text(
                                            text = "Chart data available: ${chart.data.size} entries",
                                            style = MaterialTheme.typography.bodyMedium
                                        )
                                        Text(
                                            text = "Total Gross: ${
                                                chart.data.sumOf { it.gross }.formatCurrency()
                                            }",
                                            style = MaterialTheme.typography.bodyMedium,
                                            modifier = Modifier.padding(top = 8.dp)
                                        )
                                        Text(
                                            text = "Total Driver Share: ${
                                                chart.data.sumOf { it.driverShare }.formatCurrency()
                                            }",
                                            style = MaterialTheme.typography.bodyMedium
                                        )
                                    }
                                }

                                is ChartUiState.Error -> {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(16.dp)
                                    ) {
                                        Text(
                                            text = chart.message,
                                            color = MaterialTheme.colorScheme.error
                                        )
                                    }
                                }
                            }
                        }
                    }
                }
            }

            is StatsUiState.Error -> {
                ErrorView(
                    message = state.message,
                    onRetry = { viewModel.refresh() }
                )
            }
        }
    }

    if (showRangePicker) {
        AlertDialog(
            onDismissRequest = { showRangePicker = false },
            title = { Text("Select Date Range") },
            text = {
                Column {
                    dateRanges.forEach { range ->
                        TextButton(
                            onClick = {
                                viewModel.selectDateRange(range)
                                showRangePicker = false
                            },
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text(range.name)
                        }
                    }
                }
            },
            confirmButton = {
                TextButton(onClick = { showRangePicker = false }) {
                    Text("Cancel")
                }
            }
        )
    }
}

@Composable
private fun StatItem(label: String, value: String) {
    Column {
        Text(
            text = label,
            style = MaterialTheme.typography.labelMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Spacer(modifier = Modifier.height(4.dp))
        Text(
            text = value,
            style = MaterialTheme.typography.titleMedium,
            fontWeight = FontWeight.Bold,
            color = MaterialTheme.colorScheme.primary
        )
    }
}
