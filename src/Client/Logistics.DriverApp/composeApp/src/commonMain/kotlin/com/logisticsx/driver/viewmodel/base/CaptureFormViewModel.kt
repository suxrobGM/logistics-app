package com.logisticsx.driver.viewmodel.base

import com.logisticsx.driver.service.LocationService
import com.logisticsx.driver.ui.components.PathData
import com.logisticsx.driver.viewmodel.CapturedPhoto
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update

/**
 * Common fields shared by all capture/inspection form state classes.
 */
interface CaptureFormState {
    val photos: List<CapturedPhoto>
    val signaturePaths: List<PathData>?
    val signatureBase64: String?
    val notes: String
    val latitude: Double?
    val longitude: Double?
    val isSubmitting: Boolean
    val error: String?
    val isSuccess: Boolean
}

/**
 * Base ViewModel for capture forms (DVIR, POD, Condition Report).
 * Extracts the shared location fetch, photo/signature management,
 * notes, error clearing, and submit pattern that was duplicated
 * across DvirFormViewModel, PodCaptureViewModel, and ConditionReportViewModel.
 *
 * Subclasses must:
 * 1. Define their own state data class implementing [CaptureFormState]
 * 2. Provide [_formState] as the backing MutableStateFlow
 * 3. Provide [updateState] to apply copy-based mutations
 * 4. Implement [performSubmit] with form-specific API call
 */
abstract class CaptureFormViewModel<S : CaptureFormState>(
    private val locationService: LocationService
) : BaseViewModel() {

    protected abstract val _formState: MutableStateFlow<S>
    abstract val formState: StateFlow<S>

    /**
     * Apply a mutation to the form state.
     * Subclasses implement this as: `_formState.update { it.copy(...) }`
     */
    protected abstract fun updateState(transform: S.() -> S)

    /**
     * Perform the form-specific submission logic.
     * Called after isSubmitting is set to true and error is cleared.
     * Should throw on failure.
     */
    protected abstract suspend fun performSubmit()

    /**
     * Whether the form has enough data to submit.
     */
    abstract fun canSubmit(): Boolean

    protected fun fetchCurrentLocation() {
        launchSafely {
            val location = locationService.getCurrentLocation()
            if (location != null) {
                updateState {
                    @Suppress("UNCHECKED_CAST")
                    copyWithLocation(location.latitude, location.longitude)
                }
            }
        }
    }

    /**
     * Subclasses implement to copy state with new lat/lng values.
     */
    protected abstract fun S.copyWithLocation(lat: Double, lng: Double): S

    fun addPhoto(photo: CapturedPhoto) {
        updateState {
            @Suppress("UNCHECKED_CAST")
            copyWithPhotos(photos + photo)
        }
    }

    fun removePhoto(photoId: String) {
        updateState {
            @Suppress("UNCHECKED_CAST")
            copyWithPhotos(photos.filter { it.id != photoId })
        }
    }

    protected abstract fun S.copyWithPhotos(photos: List<CapturedPhoto>): S

    fun setSignature(paths: List<PathData>, base64: String) {
        updateState {
            @Suppress("UNCHECKED_CAST")
            copyWithSignature(paths, base64)
        }
    }

    fun clearSignature() {
        updateState {
            @Suppress("UNCHECKED_CAST")
            copyWithSignature(null, null)
        }
    }

    protected abstract fun S.copyWithSignature(paths: List<PathData>?, base64: String?): S

    fun setNotes(notes: String) {
        updateState {
            @Suppress("UNCHECKED_CAST")
            copyWithNotes(notes)
        }
    }

    protected abstract fun S.copyWithNotes(notes: String): S

    fun clearError() {
        updateState {
            @Suppress("UNCHECKED_CAST")
            copyWithError(null)
        }
    }

    protected abstract fun S.copyWithError(error: String?): S
    protected abstract fun S.copyWithSubmitting(isSubmitting: Boolean, error: String? = this.error, isSuccess: Boolean = this.isSuccess): S

    fun submit() {
        if (!canSubmit()) return

        launchSafely(onError = { e ->
            updateState {
                copyWithSubmitting(isSubmitting = false, error = e.message ?: "Submission failed")
            }
        }) {
            updateState {
                copyWithSubmitting(isSubmitting = true, error = null)
            }
            performSubmit()
            updateState {
                copyWithSubmitting(isSubmitting = false, isSuccess = true)
            }
        }
    }
}
