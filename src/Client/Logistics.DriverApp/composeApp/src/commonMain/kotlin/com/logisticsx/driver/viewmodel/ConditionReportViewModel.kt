package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.InspectionsApi
import com.logisticsx.driver.api.LoadApi
import com.logisticsx.driver.api.VinsApi
import com.logisticsx.driver.api.bodyOrThrow
import com.logisticsx.driver.api.models.CargoInspectionPartCategory
import com.logisticsx.driver.api.models.DefectSeverity
import com.logisticsx.driver.api.models.InspectionType
import com.logisticsx.driver.api.models.LoadType
import com.logisticsx.driver.api.models.VehicleInfoDto
import com.logisticsx.driver.model.FileUploadData
import com.logisticsx.driver.model.toFormParts
import com.logisticsx.driver.service.LocationService
import com.logisticsx.driver.ui.components.PathData
import com.logisticsx.driver.util.isContainerLoad
import com.logisticsx.driver.viewmodel.base.CaptureFormState
import com.logisticsx.driver.viewmodel.base.CaptureFormViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json

/**
 * A single defect captured during cargo inspection. Mirrors the server-side
 * `ConditionDefect` value object — category drawn from the cargo-type-specific
 * catalog ([CargoInspectionPartCategory]).
 */
@Serializable
data class ConditionDefect(
    val partCategory: CargoInspectionPartCategory,
    val description: String,
    val severity: DefectSeverity
)

data class ConditionReportUiState(
    val loadId: String = "",
    val inspectionType: InspectionType = InspectionType.PICKUP,
    val cargoType: LoadType = LoadType.GENERAL_FREIGHT,
    val isLoadingLoad: Boolean = true,

    // Vehicle-cargo identifier
    val vin: String = "",
    val vehicleInfo: VehicleInfoDto? = null,
    val isDecodingVin: Boolean = false,

    // Container-cargo identifier
    val containerNumber: String = "",
    val sealNumber: String = "",

    val defects: List<ConditionDefect> = emptyList(),
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
    private val inspectionsApi: InspectionsApi,
    private val vinsApi: VinsApi,
    private val loadApi: LoadApi,
    locationService: LocationService,
    private val loadId: String,
    private val inspectionType: InspectionType
) : CaptureFormViewModel<ConditionReportUiState>(locationService) {

    override val _formState = MutableStateFlow(
        ConditionReportUiState(loadId = loadId, inspectionType = inspectionType)
    )
    override val formState: StateFlow<ConditionReportUiState> = _formState.asStateFlow()

    val uiState: StateFlow<ConditionReportUiState> get() = formState

    init {
        fetchCurrentLocation()
        loadCargoType()
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

    private fun loadCargoType() {
        launchSafely(onError = { e ->
            _formState.update {
                it.copy(isLoadingLoad = false, error = "Failed to load: ${e.message}")
            }
        }) {
            val load = loadApi.getLoadById(loadId).bodyOrThrow()
            _formState.update {
                it.copy(
                    cargoType = load.type ?: LoadType.GENERAL_FREIGHT,
                    containerNumber = load.containerNumber ?: it.containerNumber,
                    isLoadingLoad = false
                )
            }
        }
    }

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
            val vehicleInfo = vinsApi.decodeVin(vin = vin).body()
            _formState.update { it.copy(vehicleInfo = vehicleInfo, isDecodingVin = false) }
        }
    }

    fun setContainerNumber(value: String) {
        _formState.update { it.copy(containerNumber = value.uppercase()) }
    }

    fun setSealNumber(value: String) {
        _formState.update { it.copy(sealNumber = value) }
    }

    fun addDefect(defect: ConditionDefect) {
        _formState.update { state -> state.copy(defects = state.defects + defect) }
    }

    fun removeDefect(index: Int) {
        _formState.update { state ->
            state.copy(defects = state.defects.filterIndexed { i, _ -> i != index })
        }
    }

    override fun canSubmit(): Boolean {
        val state = _formState.value
        if (state.isSubmitting || state.isLoadingLoad) return false

        return when {
            state.cargoType == LoadType.VEHICLE -> state.vin.length == 17
            state.cargoType.isContainerLoad -> state.containerNumber.isNotBlank()
            else -> true
        }
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

        val defectsJson = if (state.defects.isNotEmpty()) {
            Json.encodeToString(state.defects)
        } else null

        inspectionsApi.createInspection(
            loadId = state.loadId,
            type = state.inspectionType,
            vin = state.vin.takeIf { state.cargoType == LoadType.VEHICLE && it.isNotBlank() },
            vehicleYear = state.vehicleInfo?.year?.takeIf { state.cargoType == LoadType.VEHICLE },
            vehicleMake = state.vehicleInfo?.make?.takeIf { state.cargoType == LoadType.VEHICLE },
            vehicleModel = state.vehicleInfo?.model?.takeIf { state.cargoType == LoadType.VEHICLE },
            vehicleBodyClass = state.vehicleInfo?.bodyClass?.takeIf { state.cargoType == LoadType.VEHICLE },
            containerNumber = state.containerNumber.takeIf { state.cargoType.isContainerLoad && it.isNotBlank() },
            sealNumber = state.sealNumber.takeIf { state.cargoType.isContainerLoad && it.isNotBlank() },
            defectsJson = defectsJson,
            notes = state.notes.takeIf { it.isNotBlank() },
            signatureBase64 = state.signatureBase64,
            photos = photoFormParts,
            latitude = state.latitude,
            longitude = state.longitude
        )
    }
}
