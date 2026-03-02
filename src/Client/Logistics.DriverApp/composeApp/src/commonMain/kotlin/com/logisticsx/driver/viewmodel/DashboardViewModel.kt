package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.DriverApi
import com.logisticsx.driver.api.TripApi
import com.logisticsx.driver.api.TruckApi
import com.logisticsx.driver.api.bodyOrThrow
import com.logisticsx.driver.api.models.SetDriverDeviceTokenCommand
import com.logisticsx.driver.api.models.TripDto
import com.logisticsx.driver.api.models.TripStatus
import com.logisticsx.driver.api.models.TruckDto
import com.logisticsx.driver.model.fullName
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.auth.AuthService
import com.logisticsx.driver.viewmodel.base.BaseViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

data class DashboardData(
    val truck: TruckDto,
    val trips: List<TripDto> = emptyList()
)

class DashboardViewModel(
    private val truckApi: TruckApi,
    private val tripApi: TripApi,
    private val driverApi: DriverApi,
    private val preferencesManager: PreferencesManager,
    private val authService: AuthService
) : BaseViewModel() {

    private val _uiState = MutableStateFlow<UiState<DashboardData>>(UiState.Loading)
    val uiState: StateFlow<UiState<DashboardData>> = _uiState.asStateFlow()

    init {
        loadDashboard()
    }

    fun loadDashboard() {
        launchWithState(_uiState) {
            val userId = preferencesManager.getUserId()
            if (userId.isNullOrEmpty()) {
                throw IllegalStateException("User ID not available")
            }

            val driver = driverApi.getDriverByUserId(userId).bodyOrThrow()
            val driverId = driver.id ?: ""

            val truck = truckApi.getTruckById(
                driverId,
                includeLoads = true,
                onlyActiveLoads = true
            ).bodyOrThrow()

            val tripsResponse = tripApi.getTrips(orderBy = "-CreatedAt").bodyOrThrow()
            val allTrips = tripsResponse?.items ?: emptyList()
            val trips = allTrips.filter { trip ->
                trip.status in listOf(TripStatus.DISPATCHED, TripStatus.IN_TRANSIT)
            }

            preferencesManager.saveTruckId(truck.id ?: "")
            preferencesManager.saveDriverName(truck.mainDriver?.fullName() ?: "")
            preferencesManager.saveTruckNumber(truck.number ?: "")

            DashboardData(truck, trips)
        }
    }

    fun sendDeviceToken(token: String) {
        launchSafely {
            val userId = preferencesManager.getUserId() ?: return@launchSafely
            driverApi.setDriverDeviceToken(
                userId,
                SetDriverDeviceTokenCommand(userId, token)
            ).bodyOrThrow()
        }
    }

    fun logout() {
        launchSafely {
            authService.logout()
        }
    }

    fun refresh() {
        loadDashboard()
    }
}
