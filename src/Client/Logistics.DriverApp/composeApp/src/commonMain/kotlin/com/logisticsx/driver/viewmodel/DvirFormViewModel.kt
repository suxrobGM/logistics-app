package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.DvirApi
import com.logisticsx.driver.api.TruckApi
import com.logisticsx.driver.api.models.CreateDvirDefectRequest
import com.logisticsx.driver.api.models.CreateDvirReportCommand
import com.logisticsx.driver.api.models.DefectSeverity
import com.logisticsx.driver.api.models.DvirInspectionCategory
import com.logisticsx.driver.api.models.DvirType
import com.logisticsx.driver.api.models.TruckDto
import com.logisticsx.driver.service.LocationService
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.ui.components.PathData
import com.logisticsx.driver.viewmodel.base.CaptureFormViewModel
import com.logisticsx.driver.viewmodel.base.CaptureFormState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update

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
    override val photos: List<CapturedPhoto> = emptyList(),
    override val signaturePaths: List<PathData>? = null,
    override val signatureBase64: String? = null,
    override val notes: String = "",
    override val latitude: Double? = null,
    override val longitude: Double? = null,
    override val isSubmitting: Boolean = false,
    override val error: String? = null,
    override val isSuccess: Boolean = false
) : CaptureFormState

class DvirFormViewModel(
    private val truckApi: TruckApi,
    private val dvirApi: DvirApi,
    locationService: LocationService,
    private val preferencesManager: PreferencesManager,
    private val truckId: String?,
    private val tripId: String?
) : CaptureFormViewModel<DvirFormUiState>(locationService) {

    override val _formState = MutableStateFlow(
        DvirFormUiState(truckId = truckId, tripId = tripId)
    )
    override val formState: StateFlow<DvirFormUiState> = _formState.asStateFlow()

    // Keep backward-compatible property name
    val uiState: StateFlow<DvirFormUiState> get() = formState

    init {
        loadTrucks()
        fetchCurrentLocation()
    }

    override fun updateState(transform: DvirFormUiState.() -> DvirFormUiState) {
        _formState.update { it.transform() }
    }

    override fun DvirFormUiState.copyWithLocation(lat: Double, lng: Double) =
        copy(latitude = lat, longitude = lng)
    override fun DvirFormUiState.copyWithPhotos(photos: List<CapturedPhoto>) =
        copy(photos = photos)
    override fun DvirFormUiState.copyWithSignature(paths: List<PathData>?, base64: String?) =
        copy(signaturePaths = paths, signatureBase64 = base64)
    override fun DvirFormUiState.copyWithNotes(notes: String) =
        copy(notes = notes)
    override fun DvirFormUiState.copyWithError(error: String?) =
        copy(error = error)
    override fun DvirFormUiState.copyWithSubmitting(isSubmitting: Boolean, error: String?, isSuccess: Boolean) =
        copy(isSubmitting = isSubmitting, error = error, isSuccess = isSuccess)

    private fun loadTrucks() {
        launchSafely(onError = { e ->
            _formState.update {
                it.copy(isLoadingTrucks = false, error = "Failed to load trucks: ${e.message}")
            }
        }) {
            _formState.update { it.copy(isLoadingTrucks = true) }
            val response = truckApi.getTrucks(pageSize = 100)
            val trucks = response.body()?.items ?: emptyList()
            _formState.update { state ->
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
        }
    }

    fun selectTruck(truck: TruckDto) {
        _formState.update { it.copy(selectedTruck = truck) }
    }

    fun setDvirType(type: DvirType) {
        _formState.update { it.copy(dvirType = type) }
    }

    fun setOdometerReading(value: String) {
        if (value.isEmpty() || value.all { it.isDigit() }) {
            _formState.update { it.copy(odometerReading = value) }
        }
    }

    fun addDefect(defect: DvirDefect) {
        _formState.update { state -> state.copy(defects = state.defects + defect) }
    }

    fun removeDefect(index: Int) {
        _formState.update { state ->
            state.copy(defects = state.defects.filterIndexed { i, _ -> i != index })
        }
    }

    override fun canSubmit(): Boolean {
        val state = _formState.value
        return !state.isSubmitting &&
            state.selectedTruck != null &&
            state.signatureBase64 != null
    }

    override suspend fun performSubmit() {
        val state = _formState.value
        val driverId = preferencesManager.getUserId()
            ?: throw IllegalStateException("Driver ID not found. Please log in again.")

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
        if (!response.success) {
            throw RuntimeException("Failed to submit DVIR")
        }
    }
}
