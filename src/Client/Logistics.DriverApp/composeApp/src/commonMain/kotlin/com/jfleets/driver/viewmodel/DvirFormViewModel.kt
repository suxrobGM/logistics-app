package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.DvirApi
import com.jfleets.driver.api.TruckApi
import com.jfleets.driver.api.models.CreateDvirDefectRequest
import com.jfleets.driver.api.models.CreateDvirReportCommand
import com.jfleets.driver.api.models.DefectSeverity
import com.jfleets.driver.api.models.DvirInspectionCategory
import com.jfleets.driver.api.models.DvirType
import com.jfleets.driver.api.models.TruckDto
import com.jfleets.driver.service.LocationService
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.ui.components.PathData
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class DvirDefect(
    val category: DvirInspectionCategory,
    val description: String,
    val severity: DefectSeverity
)

data class DvirFormUiState(
    val truckId: String? = null,
    val tripId: String? = null,
    val selectedTruck: TruckDto? = null,
    val availableTrucks: List<TruckDto> = emptyList(),
    val isLoadingTrucks: Boolean = false,
    val dvirType: DvirType = DvirType.PRE_TRIP,
    val odometerReading: String = "",
    val defects: List<DvirDefect> = emptyList(),
    val photos: List<CapturedPhoto> = emptyList(),
    val signaturePaths: List<PathData>? = null,
    val signatureBase64: String? = null,
    val notes: String = "",
    val latitude: Double? = null,
    val longitude: Double? = null,
    val isSubmitting: Boolean = false,
    val error: String? = null,
    val isSuccess: Boolean = false
)

class DvirFormViewModel(
    private val truckApi: TruckApi,
    private val dvirApi: DvirApi,
    private val locationService: LocationService,
    private val preferencesManager: PreferencesManager,
    private val truckId: String?,
    private val tripId: String?
) : ViewModel() {

    private val _uiState = MutableStateFlow(
        DvirFormUiState(
            truckId = truckId,
            tripId = tripId
        )
    )
    val uiState: StateFlow<DvirFormUiState> = _uiState.asStateFlow()

    init {
        loadTrucks()
        fetchCurrentLocation()
    }

    private fun loadTrucks() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoadingTrucks = true) }
            try {
                val response = truckApi.getTrucks(pageSize = 100)
                val trucks = response.body()?.items ?: emptyList()
                _uiState.update { state ->
                    state.copy(
                        availableTrucks = trucks,
                        isLoadingTrucks = false,
                        selectedTruck = if (truckId != null) {
                            trucks.find { it.id == truckId }
                        } else {
                            state.selectedTruck
                        }
                    )
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isLoadingTrucks = false,
                        error = "Failed to load trucks: ${e.message}"
                    )
                }
            }
        }
    }

    private fun fetchCurrentLocation() {
        viewModelScope.launch {
            try {
                val location = locationService.getCurrentLocation()
                if (location != null) {
                    _uiState.update {
                        it.copy(
                            latitude = location.latitude,
                            longitude = location.longitude
                        )
                    }
                }
            } catch (e: Exception) {
                // Location fetch failed, continue without location
            }
        }
    }

    fun selectTruck(truck: TruckDto) {
        _uiState.update { it.copy(selectedTruck = truck) }
    }

    fun setDvirType(type: DvirType) {
        _uiState.update { it.copy(dvirType = type) }
    }

    fun setOdometerReading(value: String) {
        // Only allow digits
        if (value.isEmpty() || value.all { it.isDigit() }) {
            _uiState.update { it.copy(odometerReading = value) }
        }
    }

    fun addDefect(defect: DvirDefect) {
        _uiState.update { state ->
            state.copy(defects = state.defects + defect)
        }
    }

    fun removeDefect(index: Int) {
        _uiState.update { state ->
            state.copy(defects = state.defects.filterIndexed { i, _ -> i != index })
        }
    }

    fun addPhoto(photo: CapturedPhoto) {
        _uiState.update { state ->
            state.copy(photos = state.photos + photo)
        }
    }

    fun removePhoto(photoId: String) {
        _uiState.update { state ->
            state.copy(photos = state.photos.filter { it.id != photoId })
        }
    }

    fun setSignature(paths: List<PathData>, base64: String) {
        _uiState.update { state ->
            state.copy(signaturePaths = paths, signatureBase64 = base64)
        }
    }

    fun clearSignature() {
        _uiState.update { state ->
            state.copy(signaturePaths = null, signatureBase64 = null)
        }
    }

    fun setNotes(notes: String) {
        _uiState.update { state ->
            state.copy(notes = notes)
        }
    }

    fun clearError() {
        _uiState.update { it.copy(error = null) }
    }

    fun canSubmit(): Boolean {
        val state = _uiState.value
        return !state.isSubmitting &&
            state.selectedTruck != null &&
            state.signatureBase64 != null
    }

    fun submit() {
        if (!canSubmit()) return

        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, error = null) }

            try {
                val state = _uiState.value
                val driverId = preferencesManager.getUserId()

                if (driverId == null) {
                    _uiState.update {
                        it.copy(
                            isSubmitting = false,
                            error = "Driver ID not found. Please log in again."
                        )
                    }
                    return@launch
                }

                val command = CreateDvirReportCommand(
                    truckId = state.selectedTruck!!.id!!,
                    driverId = driverId,
                    type = state.dvirType,
                    latitude = state.latitude,
                    longitude = state.longitude,
                    odometerReading = state.odometerReading.toIntOrNull(),
                    driverNotes = state.notes.takeIf { it.isNotBlank() },
                    driverSignature = state.signatureBase64,
                    tripId = state.tripId,
                    defects = if (state.defects.isNotEmpty()) {
                        state.defects.map { defect ->
                            CreateDvirDefectRequest(
                                category = defect.category,
                                description = defect.description,
                                severity = defect.severity
                            )
                        }
                    } else null
                )

                val response = dvirApi.createDvirReport(command)

                if (response.success) {
                    _uiState.update { it.copy(isSubmitting = false, isSuccess = true) }
                } else {
                    _uiState.update {
                        it.copy(
                            isSubmitting = false,
                            error = "Failed to submit DVIR"
                        )
                    }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isSubmitting = false,
                        error = e.message ?: "Failed to submit DVIR report"
                    )
                }
            }
        }
    }
}
