package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.api.LoadApi
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.model.Load
import kotlinx.datetime.TimeZone
import kotlinx.datetime.toLocalDateTime
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlin.time.Clock
import kotlin.time.Duration.Companion.days

class PastLoadsViewModel(
    private val loadApi: LoadApi
) : ViewModel() {

    private val _uiState = MutableStateFlow<PastLoadsUiState>(PastLoadsUiState.Loading)
    val uiState: StateFlow<PastLoadsUiState> = _uiState.asStateFlow()

    init {
        loadPastLoads()
    }

    private fun loadPastLoads() {
        viewModelScope.launch {
            _uiState.value = PastLoadsUiState.Loading

            val now = Clock.System.now()
            val ninetyDaysAgo = now.minus(90.days)
            val endDate = now.toLocalDateTime(TimeZone.UTC).date.toString()
            val startDate = ninetyDaysAgo.toLocalDateTime(TimeZone.UTC).date.toString()

            val result = loadApi.getPastLoads(startDate, endDate)
            if (result.success && result.data != null) {
                val loads = result.data.map { it.toDomain() }
                _uiState.value = PastLoadsUiState.Success(loads)
            } else {
                _uiState.value = PastLoadsUiState.Error(result.error ?: "Failed to load past loads")
            }
        }
    }

    fun refresh() {
        loadPastLoads()
    }
}

sealed class PastLoadsUiState {
    object Loading : PastLoadsUiState()
    data class Success(val loads: List<Load>) : PastLoadsUiState()
    data class Error(val message: String) : PastLoadsUiState()
}
