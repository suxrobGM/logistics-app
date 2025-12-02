package com.jfleets.driver.ui.screens

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
import androidx.compose.material3.FilterChip
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.model.ChartData
import com.jfleets.driver.model.LocalUserSettings
import com.jfleets.driver.model.toChartData
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.CardContainer
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.ui.components.charts.BarChart
import com.jfleets.driver.ui.components.charts.LineChart
import com.jfleets.driver.util.formatCurrency
import com.jfleets.driver.util.formatDistance
import com.jfleets.driver.viewmodel.ChartUiState
import com.jfleets.driver.viewmodel.StatsUiState
import com.jfleets.driver.viewmodel.StatsViewModel
import org.koin.compose.viewmodel.koinViewModel

enum class ChartType(val label: String) {
    BAR("Bar"),
    LINE("Line")
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun StatsScreen(
    viewModel: StatsViewModel = koinViewModel()
) {
    val userSettings = LocalUserSettings.current
    val statsState by viewModel.statsState.collectAsState()
    val chartState by viewModel.chartState.collectAsState()
    val selectedRange by viewModel.selectedRange.collectAsState()
    val dateRanges = remember { viewModel.getDateRanges() }
    var showRangePicker by remember { mutableStateOf(false) }
    var selectedChartType by remember { mutableStateOf(ChartType.BAR) }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "My Stats",
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
                                StatItem("Gross", stats.thisWeekGross?.formatCurrency() ?: "-")
                                StatItem("Income", stats.thisWeekShare?.formatCurrency() ?: "-")
                                StatItem(
                                    "Distance",
                                    stats.thisWeekDistance?.formatDistance(userSettings.distanceUnit) ?: "-"
                                )
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
                                StatItem("Gross", stats.thisMonthGross?.formatCurrency() ?: "-")
                                StatItem("Income", stats.thisMonthShare?.formatCurrency() ?: "-")
                                StatItem(
                                    "Distance",
                                    stats.thisMonthDistance?.formatDistance(userSettings.distanceUnit) ?: "-"
                                )
                            }
                        }
                    }

                    // Chart Section Header
                    item {
                        Column {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
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

                            Spacer(modifier = Modifier.height(8.dp))

                            // Chart type selector
                            Row(
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                ChartType.entries.forEach { chartType ->
                                    FilterChip(
                                        selected = selectedChartType == chartType,
                                        onClick = { selectedChartType = chartType },
                                        label = { Text(chartType.label) }
                                    )
                                }
                            }
                        }
                    }

                    // Chart Content
                    item {
                        CardContainer {
                            when (val chart = chartState) {
                                is ChartUiState.Loading -> {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .height(280.dp),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        LoadingIndicator()
                                    }
                                }

                                is ChartUiState.DailySuccess, is ChartUiState.MonthlySuccess -> {
                                    val chartData: List<ChartData> = when (chart) {
                                        is ChartUiState.DailySuccess -> chart.data.map { it.toChartData() }
                                        is ChartUiState.MonthlySuccess -> chart.data.map { it.toChartData() }
                                        else -> emptyList()
                                    }

                                    Column(modifier = Modifier.padding(16.dp)) {
                                        // Summary row
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(bottom = 16.dp),
                                            horizontalArrangement = Arrangement.SpaceEvenly
                                        ) {
                                            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                                Text(
                                                    text = "Total Gross",
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                                )
                                                Text(
                                                    text = chartData.sumOf { it.gross }
                                                        .formatCurrency(),
                                                    style = MaterialTheme.typography.titleMedium,
                                                    fontWeight = FontWeight.Bold,
                                                    color = MaterialTheme.colorScheme.primary
                                                )
                                            }
                                            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                                Text(
                                                    text = "Total Income",
                                                    style = MaterialTheme.typography.labelSmall,
                                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                                )
                                                Text(
                                                    text = chartData.sumOf { it.driverShare }
                                                        .formatCurrency(),
                                                    style = MaterialTheme.typography.titleMedium,
                                                    fontWeight = FontWeight.Bold,
                                                    color = MaterialTheme.colorScheme.secondary
                                                )
                                            }
                                        }

                                        // Chart display
                                        when (selectedChartType) {
                                            ChartType.BAR -> {
                                                BarChart(
                                                    data = chartData,
                                                    modifier = Modifier.fillMaxWidth()
                                                )
                                            }

                                            ChartType.LINE -> {
                                                LineChart(
                                                    data = chartData,
                                                    modifier = Modifier.fillMaxWidth()
                                                )
                                            }
                                        }
                                    }
                                }

                                is ChartUiState.Error -> {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .height(200.dp)
                                            .padding(16.dp),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                            Text(
                                                text = chart.message,
                                                color = MaterialTheme.colorScheme.error,
                                                style = MaterialTheme.typography.bodyMedium
                                            )
                                            Spacer(modifier = Modifier.height(8.dp))
                                            TextButton(onClick = { viewModel.refresh() }) {
                                                Text("Retry")
                                            }
                                        }
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
