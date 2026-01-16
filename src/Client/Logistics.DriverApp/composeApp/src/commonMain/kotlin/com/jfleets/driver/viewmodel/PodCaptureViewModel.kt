package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.DocumentApi
import com.jfleets.driver.model.FileUploadData
import com.jfleets.driver.model.toFormParts
import com.jfleets.driver.service.LocationService
import com.jfleets.driver.ui.components.PathData
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

enum class DocumentCaptureType {
    POD,  // Proof of Delivery
    BOL   // Bill of Lading
}

data class PodCaptureUiState(
    val loadId: String = "",
    val tripStopId: String? = null,
    val captureType: DocumentCaptureType = DocumentCaptureType.POD,
    val photos: List<CapturedPhoto> = emptyList(),
    val signaturePaths: List<PathData>? = null,
    val signatureBase64: String? = null,
    val recipientName: String = "",
    val notes: String = "",
    val latitude: Double? = null,
    val longitude: Double? = null,
    val isLoading: Boolean = false,
    val isSubmitting: Boolean = false,
    val error: String? = null,
    val isSuccess: Boolean = false
)

data class CapturedPhoto(
    val id: String,
    val bytes: ByteArray,
    val fileName: String,
    val contentType: String = "image/jpeg"
)

class PodCaptureViewModel(
    private val documentApi: DocumentApi,
    private val locationService: LocationService,
    private val loadId: String,
    private val tripStopId: String?,
    private val captureType: DocumentCaptureType
) : ViewModel() {

    private val _uiState = MutableStateFlow(
        PodCaptureUiState(
            loadId = loadId,
            tripStopId = tripStopId,
            captureType = captureType
        )
    )
    val uiState: StateFlow<PodCaptureUiState> = _uiState.asStateFlow()

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

    fun setRecipientName(name: String) {
        _uiState.update { state ->
            state.copy(recipientName = name)
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
                (state.photos.isNotEmpty() || state.signatureBase64 != null || state.recipientName.isNotBlank())
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

                _uiState.update { it.copy(isSubmitting = false, isSuccess = true) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isSubmitting = false,
                        error = e.message ?: "Failed to submit document"
                    )
                }
            }
        }
    }
}
