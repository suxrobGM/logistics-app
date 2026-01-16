package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.InspectionApi
import com.jfleets.driver.api.models.DecodeVinRequest
import com.jfleets.driver.api.models.InspectionType
import com.jfleets.driver.api.models.VehicleInfoDto
import com.jfleets.driver.model.FileUploadData
import com.jfleets.driver.model.toFormParts
import com.jfleets.driver.service.LocationService
import com.jfleets.driver.ui.components.PathData
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
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

class ConditionReportViewModel(
    private val inspectionApi: InspectionApi,
    private val locationService: LocationService,
    private val loadId: String,
    private val inspectionType: InspectionType
) : ViewModel() {

    private val _uiState = MutableStateFlow(
        ConditionReportUiState(
            loadId = loadId,
            inspectionType = inspectionType
        )
    )
    val uiState: StateFlow<ConditionReportUiState> = _uiState.asStateFlow()

    init {
        fetchCurrentLocation()
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

    fun setVin(vin: String) {
        _uiState.update { it.copy(vin = vin.uppercase()) }
    }

    fun decodeVin() {
        val vin = _uiState.value.vin
        if (vin.length != 17) {
            _uiState.update { it.copy(error = "VIN must be exactly 17 characters") }
            return
        }

        viewModelScope.launch {
            _uiState.update { it.copy(isDecodingVin = true, error = null) }

            try {
                val vehicleInfo = inspectionApi.decodeVin(DecodeVinRequest(vin = vin)).body()
                _uiState.update { it.copy(vehicleInfo = vehicleInfo, isDecodingVin = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isDecodingVin = false,
                        error = "Failed to decode VIN: ${e.message}"
                    )
                }
            }
        }
    }

    fun addDamageMarker(
        x: Double,
        y: Double,
        description: String? = null,
        severity: String? = null
    ) {
        val marker = DamageMarker(x, y, description, severity)
        _uiState.update { state ->
            state.copy(damageMarkers = state.damageMarkers + marker)
        }
    }

    fun removeDamageMarker(index: Int) {
        _uiState.update { state ->
            state.copy(damageMarkers = state.damageMarkers.filterIndexed { i, _ -> i != index })
        }
    }

    fun updateDamageMarker(index: Int, description: String?, severity: String?) {
        _uiState.update { state ->
            val updatedMarkers = state.damageMarkers.toMutableList()
            if (index in updatedMarkers.indices) {
                val marker = updatedMarkers[index]
                updatedMarkers[index] = marker.copy(description = description, severity = severity)
            }
            state.copy(damageMarkers = updatedMarkers)
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
        return !state.isSubmitting && state.vin.length == 17
    }

    fun submit() {
        if (!canSubmit()) return

        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, error = null) }

            try {
                val state = _uiState.value
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

                _uiState.update { it.copy(isSubmitting = false, isSuccess = true) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isSubmitting = false,
                        error = e.message ?: "Failed to submit condition report"
                    )
                }
            }
        }
    }
}
