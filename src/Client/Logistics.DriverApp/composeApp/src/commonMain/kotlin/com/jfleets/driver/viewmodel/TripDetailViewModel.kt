package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.TripApi
import com.jfleets.driver.api.models.TripDto
import com.jfleets.driver.api.models.TripStopDto
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class TripDetailViewModel(
    private val tripApi: TripApi,
    private val tripId: String
) : ViewModel() {

    private val _uiState = MutableStateFlow<TripDetailUiState>(TripDetailUiState.Loading)
    val uiState: StateFlow<TripDetailUiState> = _uiState.asStateFlow()

    init {
        loadTripDetails()
    }

    private fun loadTripDetails() {
        viewModelScope.launch {
            _uiState.value = TripDetailUiState.Loading

            try {
                val trip = tripApi.getTripById(tripId).body()
                _uiState.value = TripDetailUiState.Success(trip)
            } catch (e: Exception) {
                _uiState.value = TripDetailUiState.Error(e.message ?: "Failed to load trip details")
            }
        }
    }

    fun refresh() {
        loadTripDetails()
    }

    fun getGoogleMapsUrl(trip: TripDto): String {
        val origin = trip.originAddress
        val destination = trip.destinationAddress
        val stops = trip.stops?.sortedBy { it.order } ?: emptyList()

        // Build URL with origin, destination, and waypoints
        val originStr = "${origin.line1 ?: ""}, ${origin.city ?: ""}, ${origin.state ?: ""}"
        val destinationStr = "${destination.line1 ?: ""}, ${destination.city ?: ""}, ${destination.state ?: ""}"

        var url = "https://www.google.com/maps/dir/?api=1" +
                "&origin=${originStr.encodeUrl()}" +
                "&destination=${destinationStr.encodeUrl()}" +
                "&travelmode=driving"

        // Add intermediate stops as waypoints (excluding first and last)
        if (stops.size > 2) {
            val waypoints = stops.drop(1).dropLast(1).joinToString("|") { stop ->
                val addr = stop.address
                "${addr.line1 ?: ""}, ${addr.city ?: ""}, ${addr.state ?: ""}"
            }
            url += "&waypoints=${waypoints.encodeUrl()}"
        }

        return url
    }

    fun getStopGoogleMapsUrl(stop: TripStopDto): String {
        val location = stop.location
        return "https://www.google.com/maps/search/?api=1&query=${location.latitude},${location.longitude}"
    }

    private fun String.encodeUrl(): String {
        return this.replace(" ", "%20")
            .replace(",", "%2C")
            .replace("|", "%7C")
    }
}

sealed class TripDetailUiState {
    object Loading : TripDetailUiState()
    data class Success(val trip: TripDto) : TripDetailUiState()
    data class Error(val message: String) : TripDetailUiState()
}
