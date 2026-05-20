package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.DocumentApi
import com.logisticsx.driver.model.FileUploadData
import com.logisticsx.driver.model.toFormParts
import com.logisticsx.driver.service.LocationService
import com.logisticsx.driver.ui.components.PathData
import com.logisticsx.driver.viewmodel.base.BaseViewModel
import com.logisticsx.driver.viewmodel.base.CaptureFormState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.serialization.Serializable

@Serializable
enum class DocumentCaptureType {
    POD,  // Proof of Delivery
    BOL   // Bill of Lading
}

data class PodCaptureUiState(
    val loadId: String = "",
    val tripStopId: String? = null,
    val captureType: DocumentCaptureType = DocumentCaptureType.POD,
    val recipientName: String = "",
    val isLoading: Boolean = false,
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

data class CapturedPhoto(
    val id: String,
    val bytes: ByteArray,
    val fileName: String,
    val contentType: String = "image/jpeg"
)

class PodCaptureViewModel(
    private val documentApi: DocumentApi,
    private val locationService: LocationService,
    loadId: String,
    tripStopId: String?,
    captureType: DocumentCaptureType
) : BaseViewModel() {

    private val _formState = MutableStateFlow(
        PodCaptureUiState(
            loadId = loadId,
            tripStopId = tripStopId,
            captureType = captureType
        )
    )
    val uiState: StateFlow<PodCaptureUiState> = _formState.asStateFlow()

    init {
        fetchCurrentLocation()
    }

    private fun fetchCurrentLocation() {
        launchSafely {
            locationService.getCurrentLocation()?.let { loc ->
                _formState.update { it.copy(latitude = loc.latitude, longitude = loc.longitude) }
            }
        }
    }

    fun addPhoto(photo: CapturedPhoto) = _formState.update { it.copy(photos = it.photos + photo) }
    fun removePhoto(photoId: String) = _formState.update { it.copy(photos = it.photos.filter { p -> p.id != photoId }) }
    fun setSignature(paths: List<PathData>, base64: String) = _formState.update { it.copy(signaturePaths = paths, signatureBase64 = base64) }
    fun clearSignature() = _formState.update { it.copy(signaturePaths = null, signatureBase64 = null) }
    fun setNotes(notes: String) = _formState.update { it.copy(notes = notes) }
    fun clearError() = _formState.update { it.copy(error = null) }

    fun setRecipientName(name: String) {
        _formState.update { it.copy(recipientName = name) }
    }

    fun canSubmit(): Boolean {
        val state = _formState.value
        return !state.isSubmitting &&
                (state.photos.isNotEmpty() || state.signatureBase64 != null || state.recipientName.isNotBlank())
    }

    fun submit() = submitForm(
        state = _formState,
        canSubmit = canSubmit(),
        setSubmitting = { isSubmitting, error, isSuccess ->
            copy(isSubmitting = isSubmitting, error = error, isSuccess = isSuccess)
        },
        perform = { performSubmit() }
    )

    private suspend fun performSubmit() {
        val state = _formState.value
        val photoFormParts = state.photos.map { photo ->
            FileUploadData(
                bytes = photo.bytes,
                fileName = photo.fileName,
                contentType = photo.contentType
            )
        }.toFormParts()

        when (state.captureType) {
            DocumentCaptureType.POD -> {
                documentApi.captureProofOfDelivery(
                    loadId = state.loadId,
                    tripStopId = state.tripStopId,
                    photos = photoFormParts,
                    signatureBase64 = state.signatureBase64,
                    recipientName = state.recipientName.takeIf { it.isNotBlank() },
                    latitude = state.latitude,
                    longitude = state.longitude,
                    notes = state.notes.takeIf { it.isNotBlank() }
                )
            }

            DocumentCaptureType.BOL -> {
                documentApi.captureBillOfLading(
                    loadId = state.loadId,
                    tripStopId = state.tripStopId,
                    photos = photoFormParts,
                    signatureBase64 = state.signatureBase64,
                    recipientName = state.recipientName.takeIf { it.isNotBlank() },
                    latitude = state.latitude,
                    longitude = state.longitude,
                    notes = state.notes.takeIf { it.isNotBlank() }
                )
            }
        }
    }
}
