package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.model.ChartData
import com.jfleets.driver.data.model.DateRange
import com.jfleets.driver.data.model.DriverStats
import com.jfleets.driver.data.repository.StatsRepository
import com.jfleets.driver.util.Result
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import java.util.*
import javax.inject.Inject

@HiltViewModel
class StatsViewModel @Inject constructor(
    private val statsRepository: StatsRepository
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
            when (val result = statsRepository.getDriverStats()) {
                is Result.Success -> {
                    _statsState.value = StatsUiState.Success(result.data)
                }
                is Result.Error -> {
                    _statsState.value = StatsUiState.Error(result.message)
                }
                else -> {}
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

            val result = if (range.useMonthly) {
                val calendar = Calendar.getInstance()
                calendar.time = range.startDate
                val startMonth = calendar.get(Calendar.MONTH) + 1
                val startYear = calendar.get(Calendar.YEAR)

                calendar.time = range.endDate
                val endMonth = calendar.get(Calendar.MONTH) + 1
                val endYear = calendar.get(Calendar.YEAR)

                statsRepository.getMonthlyGrosses(startMonth, startYear, endMonth, endYear)
            } else {
                statsRepository.getDailyGrosses(range.startDate, range.endDate)
            }

            when (result) {
                is Result.Success -> {
                    _chartState.value = ChartUiState.Success(result.data)
                }
                is Result.Error -> {
                    _chartState.value = ChartUiState.Error(result.message)
                }
                else -> {}
            }
        }
    }

    fun getDateRanges(): List<DateRange> {
        val calendar = Calendar.getInstance()
        val today = calendar.time

        // This Week
        calendar.set(Calendar.DAY_OF_WEEK, calendar.firstDayOfWeek)
        val thisWeekStart = calendar.time

        // Last Week
        calendar.add(Calendar.WEEK_OF_YEAR, -1)
        val lastWeekStart = calendar.time
        calendar.add(Calendar.DAY_OF_YEAR, 6)
        val lastWeekEnd = calendar.time

        // This Month
        calendar.time = today
        calendar.set(Calendar.DAY_OF_MONTH, 1)
        val thisMonthStart = calendar.time

        // Last Month
        calendar.add(Calendar.MONTH, -1)
        val lastMonthStart = calendar.time
        calendar.set(Calendar.DAY_OF_MONTH, calendar.getActualMaximum(Calendar.DAY_OF_MONTH))
        val lastMonthEnd = calendar.time

        // Past 90 Days
        calendar.time = today
        calendar.add(Calendar.DAY_OF_YEAR, -90)
        val past90DaysStart = calendar.time

        // This Year
        calendar.time = today
        calendar.set(Calendar.DAY_OF_YEAR, 1)
        val thisYearStart = calendar.time

        // Last Year
        calendar.add(Calendar.YEAR, -1)
        val lastYearStart = calendar.time
        calendar.set(Calendar.DAY_OF_YEAR, calendar.getActualMaximum(Calendar.DAY_OF_YEAR))
        val lastYearEnd = calendar.time

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
