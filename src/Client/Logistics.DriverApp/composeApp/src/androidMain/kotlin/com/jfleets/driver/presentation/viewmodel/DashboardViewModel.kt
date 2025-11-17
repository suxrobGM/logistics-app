package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.shared.data.repository.TruckRepository
import com.jfleets.driver.shared.data.repository.UserRepository
import com.jfleets.driver.shared.model.Truck
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class DashboardViewModel(
    private val truckRepository: TruckRepository,
    private val userRepository: UserRepository,
    private val preferencesManager: PreferencesManager,
    private val authRepository: AuthRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<DashboardUiState>(DashboardUiState.Loading)
    val uiState: StateFlow<DashboardUiState> = _uiState.asStateFlow()

    init {
        loadDashboard()
    }

    fun loadDashboard() {
        viewModelScope.launch {
            _uiState.value = DashboardUiState.Loading

            try {
                // Get driver ID first
                userRepository.getCurrentDriver()
                    .onSuccess { driverId ->
                        // Then get truck with active loads
                        truckRepository.getTruckByDriver(driverId)
                            .onSuccess { truck ->
                                _uiState.value = DashboardUiState.Success(truck)
                            }
                            .onFailure { error ->
                                _uiState.value =
                                    DashboardUiState.Error(error.message ?: "Failed to load truck")
                            }
                    }
                    .onFailure { error ->
                        _uiState.value =
                            DashboardUiState.Error(error.message ?: "Failed to load driver")
                    }
            } catch (e: Exception) {
                _uiState.value = DashboardUiState.Error(e.message ?: "An error occurred")
            }
        }
    }

    fun sendDeviceToken(token: String) {
        viewModelScope.launch {
            userRepository.sendDeviceToken(token)
        }
    }

    fun logout() {
        viewModelScope.launch {
            authRepository.logout()
        }
    }

    fun refresh() {
        loadDashboard()
    }
}

sealed class DashboardUiState {
    object Loading : DashboardUiState()
    data class Success(val truck: Truck) : DashboardUiState()
    data class Error(val message: String) : DashboardUiState()
}
