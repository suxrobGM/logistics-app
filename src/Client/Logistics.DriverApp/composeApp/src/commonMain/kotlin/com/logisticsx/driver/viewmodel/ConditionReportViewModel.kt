package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.InspectionApi
import com.logisticsx.driver.api.models.DecodeVinRequest
import com.logisticsx.driver.api.models.InspectionType
import com.logisticsx.driver.api.models.VehicleInfoDto
import com.logisticsx.driver.model.FileUploadData
import com.logisticsx.driver.model.toFormParts
import com.logisticsx.driver.service.LocationService
import com.logisticsx.driver.ui.components.PathData
import com.logisticsx.driver.viewmodel.base.CaptureFormViewModel
import com.logisticsx.driver.viewmodel.base.CaptureFormState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json

@Serializable
data class DamageMarker(
    val x: Double,
    val y: Double,
    val description: String? = null,
    val severity: String? = null
)

data class ConditionReportUiState(
    val loadId: String = "",
    val inspectionType: InspectionType = InspectionType.PICKUP,
    val vin: String = "",
    val vehicleInfo: VehicleInfoDto? = null,
    val isDecodingVin: Boolean = false,
    val damageMarkers: List<DamageMarker> = emptyList(),
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

class ConditionReportViewModel(
    private val inspectionApi: InspectionApi,
    locationService: LocationService,
    private val loadId: String,
    private val inspectionType: InspectionType
) : CaptureFormViewModel<ConditionReportUiState>(locationService) {

    override val _formState = MutableStateFlow(
        ConditionReportUiState(
            loadId = loadId,
            inspectionType = inspectionType
        )
    )
    override val formState: StateFlow<ConditionReportUiState> = _formState.asStateFlow()

    val uiState: StateFlow<ConditionReportUiState> get() = formState

    init {
        fetchCurrentLocation()
    }

    override fun updateState(transform: ConditionReportUiState.() -> ConditionReportUiState) {
        _formState.update { it.transform() }
    }

    override fun ConditionReportUiState.copyWithLocation(lat: Double, lng: Double) =
        copy(latitude = lat, longitude = lng)
    override fun ConditionReportUiState.copyWithPhotos(photos: List<CapturedPhoto>) =
        copy(photos = photos)
    override fun ConditionReportUiState.copyWithSignature(paths: List<PathData>?, base64: String?) =
        copy(signaturePaths = paths, signatureBase64 = base64)
    override fun ConditionReportUiState.copyWithNotes(notes: String) =
        copy(notes = notes)
    override fun ConditionReportUiState.copyWithError(error: String?) =
        copy(error = error)
    override fun ConditionReportUiState.copyWithSubmitting(isSubmitting: Boolean, error: String?, isSuccess: Boolean) =
        copy(isSubmitting = isSubmitting, error = error, isSuccess = isSuccess)

    fun setVin(vin: String) {
        _formState.update { it.copy(vin = vin.uppercase()) }
    }

    fun decodeVin() {
        val vin = _formState.value.vin
        if (vin.length != 17) {
            _formState.update { it.copy(error = "VIN must be exactly 17 characters") }
            return
        }

        launchSafely(onError = { e ->
            _formState.update {
                it.copy(isDecodingVin = false, error = "Failed to decode VIN: ${e.message}")
            }
        }) {
            _formState.update { it.copy(isDecodingVin = true, error = null) }
            val vehicleInfo = inspectionApi.decodeVin(DecodeVinRequest(vin = vin)).body()
            _formState.update { it.copy(vehicleInfo = vehicleInfo, isDecodingVin = false) }
        }
    }

    fun addDamageMarker(
        x: Double,
        y: Double,
        description: String? = null,
        severity: String? = null
    ) {
        val marker = DamageMarker(x, y, description, severity)
        _formState.update { state ->
            state.copy(damageMarkers = state.damageMarkers + marker)
        }
    }

    fun removeDamageMarker(index: Int) {
        _formState.update { state ->
            state.copy(damageMarkers = state.damageMarkers.filterIndexed { i, _ -> i != index })
        }
    }

    fun updateDamageMarker(index: Int, description: String?, severity: String?) {
        _formState.update { state ->
            val updatedMarkers = state.damageMarkers.toMutableList()
            if (index in updatedMarkers.indices) {
                val marker = updatedMarkers[index]
                updatedMarkers[index] = marker.copy(description = description, severity = severity)
            }
            state.copy(damageMarkers = updatedMarkers)
        }
    }

    override fun canSubmit(): Boolean {
        val state = _formState.value
        return !state.isSubmitting && state.vin.length == 17
    }

    override suspend fun performSubmit() {
        val state = _formState.value
        val photoFormParts = state.photos.map { photo ->
            FileUploadData(
                bytes = photo.bytes,
                fileName = photo.fileName,
                contentType = photo.contentType
            )
        }.toFormParts()

        val damageMarkersJson = if (state.damageMarkers.isNotEmpty()) {
            Json.encodeToString(state.damageMarkers)
        } else null

        inspectionApi.createConditionReport(
            loadId = state.loadId,
            vin = state.vin,
            type = state.inspectionType,
            vehicleYear = state.vehicleInfo?.year,
            vehicleMake = state.vehicleInfo?.make,
            vehicleModel = state.vehicleInfo?.model,
            vehicleBodyClass = state.vehicleInfo?.bodyClass,
            damageMarkersJson = damageMarkersJson,
            notes = state.notes.takeIf { it.isNotBlank() },
            signatureBase64 = state.signatureBase64,
            photos = photoFormParts,
            latitude = state.latitude,
            longitude = state.longitude
        )
    }
}
