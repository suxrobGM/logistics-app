package com.jfleets.driver.util

import android.content.Context
import android.net.Uri
import androidx.activity.ComponentActivity
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.PickVisualMediaRequest
import androidx.activity.result.contract.ActivityResultContracts
import androidx.core.content.FileProvider
import java.io.File
import java.util.UUID

/**
 * Android implementation using Activity Result APIs.
 */
actual class CameraLauncher(
    private val activity: ComponentActivity
) {
    private var pendingCameraResult: ((CaptureResult?) -> Unit)? = null
    private var pendingGalleryResult: ((CaptureResult?) -> Unit)? = null
    private var currentPhotoUri: Uri? = null

    private val takePictureLauncher: ActivityResultLauncher<Uri> =
        activity.registerForActivityResult(ActivityResultContracts.TakePicture()) { success ->
            val callback = pendingCameraResult
            pendingCameraResult = null

            if (success && currentPhotoUri != null) {
                try {
                    val bytes = activity.contentResolver.openInputStream(currentPhotoUri!!)?.use {
                        it.readBytes()
                    }
                    if (bytes != null) {
                        val fileName = "photo_${UUID.randomUUID()}.jpg"
                        callback?.invoke(CaptureResult(bytes, fileName, "image/jpeg"))
                    } else {
                        callback?.invoke(null)
                    }
                } catch (e: Exception) {
                    Logger.e("CameraLauncher", "Failed to read captured photo", e)
                    callback?.invoke(null)
                }
            } else {
                callback?.invoke(null)
            }
        }

    private val pickMediaLauncher: ActivityResultLauncher<PickVisualMediaRequest> =
        activity.registerForActivityResult(ActivityResultContracts.PickVisualMedia()) { uri ->
            val callback = pendingGalleryResult
            pendingGalleryResult = null

            if (uri != null) {
                try {
                    val bytes = activity.contentResolver.openInputStream(uri)?.use {
                        it.readBytes()
                    }
                    val contentType = activity.contentResolver.getType(uri) ?: "image/jpeg"
                    val extension = when {
                        contentType.contains("png") -> "png"
                        contentType.contains("gif") -> "gif"
                        else -> "jpg"
                    }

                    if (bytes != null) {
                        val fileName = "photo_${UUID.randomUUID()}.$extension"
                        callback?.invoke(CaptureResult(bytes, fileName, contentType))
                    } else {
                        callback?.invoke(null)
                    }
                } catch (e: Exception) {
                    Logger.e("CameraLauncher", "Failed to read selected photo", e)
                    callback?.invoke(null)
                }
            } else {
                callback?.invoke(null)
            }
        }

    actual fun launchCamera(onResult: (CaptureResult?) -> Unit) {
        try {
            pendingCameraResult = onResult

            // Create a temporary file for the photo
            val photoFile = File.createTempFile(
                "photo_${UUID.randomUUID()}",
                ".jpg",
                activity.cacheDir
            )

            currentPhotoUri = FileProvider.getUriForFile(
                activity,
                "${activity.packageName}.fileprovider",
                photoFile
            )

            takePictureLauncher.launch(currentPhotoUri!!)
        } catch (e: Exception) {
            Logger.e("CameraLauncher", "Failed to launch camera", e)
            pendingCameraResult = null
            onResult(null)
        }
    }

    actual fun launchGallery(onResult: (CaptureResult?) -> Unit) {
        try {
            pendingGalleryResult = onResult
            pickMediaLauncher.launch(
                PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly)
            )
        } catch (e: Exception) {
            Logger.e("CameraLauncher", "Failed to launch gallery", e)
            pendingGalleryResult = null
            onResult(null)
        }
    }
}
