package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.TruckApi
import com.jfleets.driver.api.models.SetDriverDeviceTokenCommand
import com.jfleets.driver.model.Truck
import com.jfleets.driver.model.toDomain
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class DashboardViewModel(
    private val truckApi: TruckApi,
    private val driverApi: DriverApi,
    private val preferencesManager: PreferencesManager,
    private val authService: AuthService
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
                val userId = preferencesManager.getUserId()
                if (userId.isNullOrEmpty()) {
                    _uiState.value = DashboardUiState.Error("User ID not available")
                    return@launch
                }

                // Get driver ID first
                val driverResponse = driverApi.getDriverByUserId(userId)
                val driverResult = driverResponse.body()
                if (driverResult.success != true || driverResult.data == null) {
                    _uiState.value =
                        DashboardUiState.Error(driverResult.error ?: "Failed to load driver")
                    return@launch
                }

                val driverId = driverResult.data.id ?: ""

                // Then get truck with active loads
                val truckResponse =
                    truckApi.getTruckById(driverId, includeLoads = true, onlyActiveLoads = true)
                val truckResult = truckResponse.body()
                if (truckResult.success == true && truckResult.data != null) {
                    _uiState.value = DashboardUiState.Success(truckResult.data.toDomain())
                } else {
                    _uiState.value =
                        DashboardUiState.Error(truckResult.error ?: "Failed to load truck")
                }
            } catch (e: Exception) {
                _uiState.value = DashboardUiState.Error(e.message ?: "An error occurred")
            }
        }
    }

    fun sendDeviceToken(token: String) {
        viewModelScope.launch {
            val userId = preferencesManager.getUserId() ?: return@launch
            driverApi.setDriverDeviceToken(userId, SetDriverDeviceTokenCommand(userId, token))
        }
    }

    fun logout() {
        viewModelScope.launch {
            authService.logout()
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
