package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.shared.domain.model.Load
import com.jfleets.driver.shared.domain.model.LoadStatus
import com.jfleets.driver.shared.data.repository.LoadRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class LoadDetailViewModel(
    private val loadRepository: LoadRepository,
    savedStateHandle: SavedStateHandle
) : ViewModel() {

    private val loadId: Double = savedStateHandle.get<String>("loadId")?.toDoubleOrNull() ?: 0.0

    private val _uiState = MutableStateFlow<LoadDetailUiState>(LoadDetailUiState.Loading)
    val uiState: StateFlow<LoadDetailUiState> = _uiState.asStateFlow()

    init {
        loadDetails()
    }

    private fun loadDetails() {
        viewModelScope.launch {
            _uiState.value = LoadDetailUiState.Loading
            loadRepository.getLoad(loadId)
                .onSuccess { load ->
                    _uiState.value = LoadDetailUiState.Success(load)
                }
                .onFailure { error ->
                    _uiState.value = LoadDetailUiState.Error(error.message ?: "Failed to load details")
                }
        }
    }

    fun confirmPickup() {
        viewModelScope.launch {
            loadRepository.confirmPickup(loadId)
                .onSuccess {
                    loadDetails() // Reload to get updated status
                }
                .onFailure { error ->
                    // Show error in UI
                }
        }
    }

    fun confirmDelivery() {
        viewModelScope.launch {
            loadRepository.confirmDelivery(loadId)
                .onSuccess {
                    loadDetails() // Reload to get updated status
                }
                .onFailure { error ->
                    // Show error in UI
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
