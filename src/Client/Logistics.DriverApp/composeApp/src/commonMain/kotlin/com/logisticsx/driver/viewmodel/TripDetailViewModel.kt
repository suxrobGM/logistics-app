package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.TripApi
import com.logisticsx.driver.api.bodyOrThrow
import com.logisticsx.driver.api.models.TripDto
import com.logisticsx.driver.api.models.TripStopDto
import com.logisticsx.driver.viewmodel.base.BaseViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

class TripDetailViewModel(
    private val tripApi: TripApi,
    private val tripId: String
) : BaseViewModel() {

    private val _uiState = MutableStateFlow<UiState<TripDto>>(UiState.Loading)
    val uiState: StateFlow<UiState<TripDto>> = _uiState.asStateFlow()

    init {
        loadTripDetails()
    }

    private fun loadTripDetails() {
        launchWithState(_uiState) {
            tripApi.getTripById(tripId).bodyOrThrow()
        }
    }

    fun refresh() {
        loadTripDetails()
    }

    fun getGoogleMapsUrl(trip: TripDto): String {
        val origin = trip.originAddress
        val destination = trip.destinationAddress
        val stops = trip.stops?.sortedBy { it.order } ?: emptyList()

        val originStr = "${origin.line1 ?: ""}, ${origin.city ?: ""}, ${origin.state ?: ""}"
        val destinationStr = "${destination.line1 ?: ""}, ${destination.city ?: ""}, ${destination.state ?: ""}"

        var url = "https://www.google.com/maps/dir/?api=1" +
                "&origin=${originStr.encodeUrl()}" +
                "&destination=${destinationStr.encodeUrl()}" +
                "&travelmode=driving"

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
