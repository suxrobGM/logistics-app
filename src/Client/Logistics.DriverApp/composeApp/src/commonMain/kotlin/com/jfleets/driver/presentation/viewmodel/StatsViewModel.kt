@file:OptIn(ExperimentalTime::class)

package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.api.StatsApi
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.mapper.toChartData
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.model.ChartData
import com.jfleets.driver.model.DateRange
import com.jfleets.driver.model.DriverStats
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlinx.datetime.DateTimeUnit
import kotlinx.datetime.LocalDate
import kotlinx.datetime.TimeZone
import kotlinx.datetime.atStartOfDayIn
import kotlinx.datetime.atTime
import kotlinx.datetime.minus
import kotlinx.datetime.number
import kotlinx.datetime.plus
import kotlinx.datetime.toInstant
import kotlinx.datetime.toLocalDateTime
import kotlin.time.Clock
import kotlin.time.Duration.Companion.days
import kotlin.time.ExperimentalTime

class StatsViewModel(
    private val statsApi: StatsApi,
    private val preferencesManager: PreferencesManager
) : ViewModel() {

    private val _statsState = MutableStateFlow<StatsUiState>(StatsUiState.Loading)
    val statsState: StateFlow<StatsUiState> = _statsState.asStateFlow()

    private val _chartState = MutableStateFlow<ChartUiState>(ChartUiState.Loading)
    val chartState: StateFlow<ChartUiState> = _chartState.asStateFlow()

    private val _selectedRange = MutableStateFlow(getDateRanges()[0])
    val selectedRange: StateFlow<DateRange> = _selectedRange.asStateFlow()

    init {
        loadStats()
        loadChartData(_selectedRange.value)
    }

    private fun loadStats() {
        viewModelScope.launch {
            _statsState.value = StatsUiState.Loading
            val userId = preferencesManager.getUserId()

            userId?.let {
                val result = statsApi.getDriverStats(it)
                if (result.success && result.data != null) {
                    _statsState.value = StatsUiState.Success(result.data.toDomain())
                } else {
                    _statsState.value = StatsUiState.Error(result.error ?: "Failed to load stats")
                }
            }
        }
    }

    fun selectDateRange(range: DateRange) {
        _selectedRange.value = range
        loadChartData(range)
    }

    private fun loadChartData(range: DateRange) {
        viewModelScope.launch {
            _chartState.value = ChartUiState.Loading

            if (range.useMonthly) {
                val startDateTime = range.startDate.toLocalDateTime(TimeZone.currentSystemDefault())
                val startMonth = startDateTime.month.number
                val startYear = startDateTime.year

                val endDateTime = range.endDate.toLocalDateTime(TimeZone.currentSystemDefault())
                val endMonth = endDateTime.month.number
                val endYear = endDateTime.year

                val result = statsApi.getMonthlyGrosses(startMonth, startYear, endMonth, endYear)

                if (result.success && result.data != null) {
                    val chartData = result.data.data.map { it.toChartData() }
                    _chartState.value = ChartUiState.Success(chartData)
                } else {
                    _chartState.value =
                        ChartUiState.Error(result.error ?: "Failed to load chart data")
                }
            } else {
                val startDate = range.startDate.toLocalDateTime(TimeZone.UTC).date.toString()
                val endDate = range.endDate.toLocalDateTime(TimeZone.UTC).date.toString()

                val result = statsApi.getDailyGrosses(startDate, endDate)
                if (result.success && result.data != null) {
                    val chartData = result.data.data.map { it.toChartData() }
                    _chartState.value = ChartUiState.Success(chartData)
                } else {
                    _chartState.value =
                        ChartUiState.Error(result.error ?: "Failed to load chart data")
                }
            }
        }
    }

    fun getDateRanges(): List<DateRange> {
        val timeZone = TimeZone.currentSystemDefault()
        val today = Clock.System.now()
        val todayLocal = today.toLocalDateTime(timeZone).date

        // This Week (start from Sunday)
        val thisWeekStart = todayLocal.minus(todayLocal.dayOfWeek.ordinal, DateTimeUnit.DAY)
            .atStartOfDayIn(timeZone)

        // Last Week
        val lastWeekStart = thisWeekStart.minus(7.days)
        val lastWeekEnd = thisWeekStart.minus(1.days)

        // This Month
        val thisMonthStart = LocalDate(todayLocal.year, todayLocal.month, 1)
            .atStartOfDayIn(timeZone)

        // Last Month
        val lastMonthDate = todayLocal.minus(1, DateTimeUnit.MONTH)
        val lastMonthStart = LocalDate(lastMonthDate.year, lastMonthDate.month, 1)
            .atStartOfDayIn(timeZone)
        // Calculate last day by going to first day of next month and subtracting one day
        val lastMonthEnd = LocalDate(lastMonthDate.year, lastMonthDate.month, 1)
            .plus(1, DateTimeUnit.MONTH)
            .minus(1, DateTimeUnit.DAY)
            .atTime(23, 59, 59)
            .toInstant(timeZone)

        // Past 90 Days
        val past90DaysStart = today.minus(90.days)

        // This Year
        val thisYearStart = LocalDate(todayLocal.year, 1, 1)
            .atStartOfDayIn(timeZone)

        // Last Year
        val lastYearStart = LocalDate(todayLocal.year - 1, 1, 1)
            .atStartOfDayIn(timeZone)
        val lastYearEnd = LocalDate(todayLocal.year - 1, 12, 31)
            .atTime(23, 59, 59)
            .toInstant(timeZone)

        return listOf(
            DateRange("This Week", thisWeekStart, today),
            DateRange("Last Week", lastWeekStart, lastWeekEnd),
            DateRange("This Month", thisMonthStart, today),
            DateRange("Last Month", lastMonthStart, lastMonthEnd),
            DateRange("Past 90 Days", past90DaysStart, today),
            DateRange("This Year", thisYearStart, today, useMonthly = true),
            DateRange("Last Year", lastYearStart, lastYearEnd, useMonthly = true)
        )
    }

    fun refresh() {
        loadStats()
        loadChartData(_selectedRange.value)
    }
}

sealed class StatsUiState {
    object Loading : StatsUiState()
    data class Success(val stats: DriverStats) : StatsUiState()
    data class Error(val message: String) : StatsUiState()
}

sealed class ChartUiState {
    object Loading : ChartUiState()
    data class Success(val data: List<ChartData>) : ChartUiState()
    data class Error(val message: String) : ChartUiState()
}
