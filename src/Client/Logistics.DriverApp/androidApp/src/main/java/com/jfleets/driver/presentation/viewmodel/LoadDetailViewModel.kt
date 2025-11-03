package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.model.Load
import com.jfleets.driver.data.model.LoadStatus
import com.jfleets.driver.data.repository.LoadRepository
import com.jfleets.driver.util.Result
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class LoadDetailViewModel @Inject constructor(
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
            when (val result = loadRepository.getLoad(loadId)) {
                is Result.Success -> {
                    _uiState.value = LoadDetailUiState.Success(result.data)
                }
                is Result.Error -> {
                    _uiState.value = LoadDetailUiState.Error(result.message)
                }
                else -> {}
            }
        }
    }

    fun confirmPickup() {
        viewModelScope.launch {
            when (loadRepository.confirmPickup(loadId)) {
                is Result.Success -> {
                    loadDetails() // Reload to get updated status
                }
                is Result.Error -> {
                    // Show error in UI
                }
                else -> {}
            }
        }
    }

    fun confirmDelivery() {
        viewModelScope.launch {
            when (loadRepository.confirmDelivery(loadId)) {
                is Result.Success -> {
                    loadDetails() // Reload to get updated status
                }
                is Result.Error -> {
                    // Show error in UI
                }
                else -> {}
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
