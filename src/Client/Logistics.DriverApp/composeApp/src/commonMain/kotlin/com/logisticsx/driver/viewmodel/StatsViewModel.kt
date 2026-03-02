@file:OptIn(kotlin.time.ExperimentalTime::class)

package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.StatApi
import com.logisticsx.driver.api.bodyOrThrow
import com.logisticsx.driver.api.models.DailyGrossDto
import com.logisticsx.driver.api.models.DriverStatsDto
import com.logisticsx.driver.api.models.MonthlyGrossDto
import com.logisticsx.driver.model.DateRange
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.viewmodel.base.BaseViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.datetime.DateTimeUnit
import kotlinx.datetime.LocalDate
import kotlinx.datetime.TimeZone
import kotlinx.datetime.atStartOfDayIn
import kotlinx.datetime.atTime
import kotlinx.datetime.minus
import kotlinx.datetime.plus
import kotlinx.datetime.toInstant
import kotlinx.datetime.toLocalDateTime
import kotlin.time.Clock
import kotlin.time.Duration.Companion.days

sealed class ChartUiState {
    data object Loading : ChartUiState()
    data class DailySuccess(val data: List<DailyGrossDto>) : ChartUiState()
    data class MonthlySuccess(val data: List<MonthlyGrossDto>) : ChartUiState()
    data class Error(val message: String) : ChartUiState()
}

class StatsViewModel(
    private val statApi: StatApi,
    private val preferencesManager: PreferencesManager
) : BaseViewModel() {

    private val _statsState = MutableStateFlow<UiState<DriverStatsDto>>(UiState.Loading)
    val statsState: StateFlow<UiState<DriverStatsDto>> = _statsState.asStateFlow()

    private val _chartState = MutableStateFlow<ChartUiState>(ChartUiState.Loading)
    val chartState: StateFlow<ChartUiState> = _chartState.asStateFlow()

    val dateRanges: List<DateRange> by lazy { buildDateRanges() }

    private val _selectedRange = MutableStateFlow(dateRanges[0])
    val selectedRange: StateFlow<DateRange> = _selectedRange.asStateFlow()

    init {
        loadStats()
        loadChartData(_selectedRange.value)
    }

    private fun loadStats() {
        launchWithState(_statsState) {
            val userId = preferencesManager.getUserId()
                ?: throw IllegalStateException("User ID not available")
            statApi.getDriverStats(userId).bodyOrThrow()
        }
    }

    fun selectDateRange(range: DateRange) {
        _selectedRange.value = range
        loadChartData(range)
    }

    private fun loadChartData(range: DateRange) {
        launchSafely(onError = { e ->
            _chartState.value = ChartUiState.Error(e.message ?: "An error occurred")
        }) {
            _chartState.value = ChartUiState.Loading
            if (range.useMonthly) {
                val response = statApi.getMonthlyGrosses(startDate = range.startDate, endDate = range.endDate).bodyOrThrow()
                _chartState.value = ChartUiState.MonthlySuccess(response.data ?: emptyList())
            } else {
                val response = statApi.getDailyGrosses(startDate = range.startDate, endDate = range.endDate).bodyOrThrow()
                _chartState.value = ChartUiState.DailySuccess(response.data ?: emptyList())
            }
        }
    }

    private fun buildDateRanges(): List<DateRange> {
        val timeZone = TimeZone.currentSystemDefault()
        val today = Clock.System.now()
        val todayLocal = today.toLocalDateTime(timeZone).date

        val thisWeekStart = todayLocal.minus(todayLocal.dayOfWeek.ordinal, DateTimeUnit.DAY)
            .atStartOfDayIn(timeZone)

        val lastWeekStart = thisWeekStart.minus(7.days)
        val lastWeekEnd = thisWeekStart.minus(1.days)

        val thisMonthStart = LocalDate(todayLocal.year, todayLocal.month, 1)
            .atStartOfDayIn(timeZone)

        val lastMonthDate = todayLocal.minus(1, DateTimeUnit.MONTH)
        val lastMonthStart = LocalDate(lastMonthDate.year, lastMonthDate.month, 1)
            .atStartOfDayIn(timeZone)
        val lastMonthEnd = LocalDate(lastMonthDate.year, lastMonthDate.month, 1)
            .plus(1, DateTimeUnit.MONTH)
            .minus(1, DateTimeUnit.DAY)
            .atTime(23, 59, 59)
            .toInstant(timeZone)

        val past90DaysStart = today.minus(90.days)

        val thisYearStart = LocalDate(todayLocal.year, 1, 1)
            .atStartOfDayIn(timeZone)

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
