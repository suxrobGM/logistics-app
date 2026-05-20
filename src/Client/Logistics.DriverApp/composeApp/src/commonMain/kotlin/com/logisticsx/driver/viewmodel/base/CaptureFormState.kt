package com.logisticsx.driver.viewmodel.base

import com.logisticsx.driver.ui.components.PathData
import com.logisticsx.driver.viewmodel.CapturedPhoto

/**
 * Common fields shared by all capture/inspection form state classes
 * (DVIR, POD, Condition Report). Lets shared UI read photos/signature/error/etc.
 * uniformly. Each form ViewModel owns its own mutations via `copy(...)`.
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
