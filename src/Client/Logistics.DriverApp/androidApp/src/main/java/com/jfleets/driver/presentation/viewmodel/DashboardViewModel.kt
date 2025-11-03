package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.model.Truck
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.data.repository.TruckRepository
import com.jfleets.driver.data.repository.UserRepository
import com.jfleets.driver.util.Result
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class DashboardViewModel @Inject constructor(
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

            // Get driver ID first
            when (val driverResult = userRepository.getCurrentDriver()) {
                is Result.Success -> {
                    val driverId = driverResult.data

                    // Then get truck with active loads
                    when (val truckResult = truckRepository.getTruckByDriver(driverId)) {
                        is Result.Success -> {
                            _uiState.value = DashboardUiState.Success(truckResult.data)
                        }
                        is Result.Error -> {
                            _uiState.value = DashboardUiState.Error(truckResult.message)
                        }
                        else -> {}
                    }
                }
                is Result.Error -> {
                    _uiState.value = DashboardUiState.Error(driverResult.message)
                }
                else -> {}
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
