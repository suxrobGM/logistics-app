@file:OptIn(ExperimentalTime::class)

package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.api.LoadApi
import com.jfleets.driver.data.dto.ConfirmLoadStatus
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.model.Load
import com.jfleets.driver.model.LoadStatus
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlin.time.ExperimentalTime

class LoadDetailViewModel(
    private val loadApi: LoadApi,
    private val loadId: Double
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoadDetailUiState>(LoadDetailUiState.Loading)
    val uiState: StateFlow<LoadDetailUiState> = _uiState.asStateFlow()

    init {
        loadDetails()
    }

    private fun loadDetails() {
        viewModelScope.launch {
            _uiState.value = LoadDetailUiState.Loading
            val result = loadApi.getLoad(loadId)
            if (result.success && result.data != null) {
                _uiState.value = LoadDetailUiState.Success(result.data.toDomain())
            } else {
                _uiState.value = LoadDetailUiState.Error(result.error ?: "Failed to load details")
            }
        }
    }

    fun confirmPickup() {
        viewModelScope.launch {
            val request = ConfirmLoadStatus(loadId, LoadStatus.PICKED_UP.toApiString())
            val result = loadApi.confirmLoadStatus(request)
            if (result.success) {
                loadDetails() // Reload to get updated status
            }
        }
    }

    fun confirmDelivery() {
        viewModelScope.launch {
            val request = ConfirmLoadStatus(loadId, LoadStatus.DELIVERED.toApiString())
            val result = loadApi.confirmLoadStatus(request)
            if (result.success) {
                loadDetails() // Reload to get updated status
            }
        }
    }

    fun refresh() {
        loadDetails()
    }

    fun getGoogleMapsUrl(load: Load): String {
        val origin = "${load.originLatitude},${load.originLongitude}"
        val destination = "${load.destinationLatitude},${load.destinationLongitude}"
        return "https://www.google.com/maps/dir/?api=1&origin=$origin&destination=$destination&travelmode=driving"
    }
}

sealed class LoadDetailUiState {
    object Loading : LoadDetailUiState()
    data class Success(val load: Load) : LoadDetailUiState()
    data class Error(val message: String) : LoadDetailUiState()
}
