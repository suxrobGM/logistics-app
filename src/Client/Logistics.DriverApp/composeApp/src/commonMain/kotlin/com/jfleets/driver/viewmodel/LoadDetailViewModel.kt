package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.api.models.ConfirmLoadStatusCommand
import com.jfleets.driver.api.models.LoadDto
import com.jfleets.driver.api.models.LoadStatus
import com.jfleets.driver.model.getGoogleMapsUrl
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class LoadDetailViewModel(
    private val loadApi: LoadApi,
    private val driverApi: DriverApi,
    private val loadId: String
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoadDetailUiState>(LoadDetailUiState.Loading)
    val uiState: StateFlow<LoadDetailUiState> = _uiState.asStateFlow()

    init {
        loadDetails()
    }

    private fun loadDetails() {
        viewModelScope.launch {
            _uiState.value = LoadDetailUiState.Loading
            try {
                val response = loadApi.getLoadById(loadId)
                val result = response.body()
                if (result.success == true && result.data != null) {
                    _uiState.value = LoadDetailUiState.Success(result.data)
                } else {
                    _uiState.value =
                        LoadDetailUiState.Error(result.error ?: "Failed to load details")
                }
            } catch (e: Exception) {
                _uiState.value = LoadDetailUiState.Error(e.message ?: "An error occurred")
            }
        }
    }

    fun confirmPickup() {
        viewModelScope.launch {
            try {
                val request = ConfirmLoadStatusCommand(
                    loadId = loadId,
                    loadStatus = LoadStatus.PICKED_UP
                )
                val response = driverApi.confirmLoadStatus(request)
                val result = response.body()
                if (result.success == true) {
                    loadDetails() // Reload to get updated status
                }
            } catch (e: Exception) {
                // Handle error
            }
        }
    }

    fun confirmDelivery() {
        viewModelScope.launch {
            try {
                val request = ConfirmLoadStatusCommand(
                    loadId = loadId,
                    loadStatus = LoadStatus.DELIVERED
                )
                val response = driverApi.confirmLoadStatus(request)
                val result = response.body()
                if (result.success == true) {
                    loadDetails() // Reload to get updated status
                }
            } catch (e: Exception) {
                // Handle error
            }
        }
    }

    fun refresh() {
        loadDetails()
    }

    fun getGoogleMapsUrl(load: LoadDto): String = load.getGoogleMapsUrl()
}

sealed class LoadDetailUiState {
    object Loading : LoadDetailUiState()
    data class Success(val load: LoadDto) : LoadDetailUiState()
    data class Error(val message: String) : LoadDetailUiState()
}
