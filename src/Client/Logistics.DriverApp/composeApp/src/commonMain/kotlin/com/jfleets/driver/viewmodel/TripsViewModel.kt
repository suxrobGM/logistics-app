package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.TripApi
import com.jfleets.driver.api.models.TripDto
import com.jfleets.driver.service.PreferencesManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class TripsViewModel(
    private val tripApi: TripApi,
    private val preferencesManager: PreferencesManager
) : ViewModel() {

    private val _uiState = MutableStateFlow<TripsUiState>(TripsUiState.Loading)
    val uiState: StateFlow<TripsUiState> = _uiState.asStateFlow()

    init {
        loadTrips()
    }

    private fun loadTrips() {
        viewModelScope.launch {
            _uiState.value = TripsUiState.Loading

            try {
                val truckId = preferencesManager.getTruckId()

                // Get all trips for this truck (active and past)
                val result = tripApi.getTrips(
                    truckId = truckId,
                    orderBy = "-CreatedAt"
                ).body()

                val trips = result?.items ?: emptyList()

                _uiState.value = TripsUiState.Success(trips)
            } catch (e: Exception) {
                _uiState.value = TripsUiState.Error(e.message ?: "Failed to load trips")
            }
        }
    }

    fun refresh() {
        loadTrips()
    }
}

sealed class TripsUiState {
    object Loading : TripsUiState()
    data class Success(val trips: List<TripDto>) : TripsUiState()
    data class Error(val message: String) : TripsUiState()
}
