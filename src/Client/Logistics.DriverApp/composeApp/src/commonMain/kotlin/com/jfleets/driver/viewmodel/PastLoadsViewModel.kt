package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.api.models.LoadDto
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

            try {
                val response = loadApi.getLoads(
                    onlyActiveLoads = false,
                    startDate = ninetyDaysAgo,
                    endDate = now
                )
                val result = response.body()
                if (result.success == true && result.data != null) {
                    _uiState.value = PastLoadsUiState.Success(result.data)
                } else {
                    _uiState.value =
                        PastLoadsUiState.Error(result.error ?: "Failed to load past loads")
                }
            } catch (e: Exception) {
                _uiState.value = PastLoadsUiState.Error(e.message ?: "An error occurred")
            }
        }
    }

    fun refresh() {
        loadPastLoads()
    }
}

sealed class PastLoadsUiState {
    object Loading : PastLoadsUiState()
    data class Success(val loads: List<LoadDto>) : PastLoadsUiState()
    data class Error(val message: String) : PastLoadsUiState()
}
