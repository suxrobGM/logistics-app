package com.logisticsx.driver.ui.components.capture

import androidx.compose.runtime.Composable
import com.logisticsx.driver.util.CameraLauncher
import com.logisticsx.driver.viewmodel.CapturedPhoto
import org.koin.compose.getKoin
import kotlin.uuid.ExperimentalUuidApi
import kotlin.uuid.Uuid

/**
 * Provides a camera capture callback that launches the camera and converts
 * the result into a [CapturedPhoto]. Returns null if no [CameraLauncher] is available.
 */
@OptIn(ExperimentalUuidApi::class)
@Composable
fun rememberCameraCapture(onPhotoCaptured: (CapturedPhoto) -> Unit): (() -> Unit)? {
    val cameraLauncher: CameraLauncher? = getKoin().getOrNull()
    return cameraLauncher?.let { launcher ->
        {
            launcher.launchCamera { result ->
                result?.let {
                    onPhotoCaptured(
                        CapturedPhoto(
                            id = Uuid.random().toString(),
                            bytes = it.bytes,
                            fileName = it.fileName,
                            contentType = it.contentType
                        )
                    )
                }
            }
        }
    }
}
