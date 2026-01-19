package com.jfleets.driver.util

import kotlinx.cinterop.ExperimentalForeignApi
import platform.Foundation.NSData
import platform.Foundation.NSUUID
import platform.UIKit.UIApplication
import platform.UIKit.UIImage
import platform.UIKit.UIImageJPEGRepresentation
import platform.UIKit.UIImagePickerController
import platform.UIKit.UIImagePickerControllerDelegateProtocol
import platform.UIKit.UIImagePickerControllerEditedImage
import platform.UIKit.UIImagePickerControllerOriginalImage
import platform.UIKit.UIImagePickerControllerSourceType
import platform.UIKit.UINavigationControllerDelegateProtocol
import platform.darwin.NSObject

/**
 * iOS implementation using UIImagePickerController.
 */
actual class CameraLauncher {
    private var pendingResult: ((CaptureResult?) -> Unit)? = null
    private var imagePickerDelegate: ImagePickerDelegate? = null

    @OptIn(ExperimentalForeignApi::class)
    actual fun launchCamera(onResult: (CaptureResult?) -> Unit) {
        if (!UIImagePickerController.isSourceTypeAvailable(UIImagePickerControllerSourceType.UIImagePickerControllerSourceTypeCamera)) {
            Logger.w("CameraLauncher", "Camera not available on this device")
            onResult(null)
            return
        }

        launchPicker(
            sourceType = UIImagePickerControllerSourceType.UIImagePickerControllerSourceTypeCamera,
            onResult = onResult
        )
    }

    @OptIn(ExperimentalForeignApi::class)
    actual fun launchGallery(onResult: (CaptureResult?) -> Unit) {
        launchPicker(
            sourceType = UIImagePickerControllerSourceType.UIImagePickerControllerSourceTypePhotoLibrary,
            onResult = onResult
        )
    }

    @OptIn(ExperimentalForeignApi::class)
    private fun launchPicker(
        sourceType: UIImagePickerControllerSourceType,
        onResult: (CaptureResult?) -> Unit
    ) {
        try {
            pendingResult = onResult

            val picker = UIImagePickerController()
            picker.sourceType = sourceType
            picker.allowsEditing = false

            imagePickerDelegate = ImagePickerDelegate { image ->
                handleImageResult(image)
            }
            picker.delegate = imagePickerDelegate

            val rootViewController = UIApplication.sharedApplication.keyWindow?.rootViewController
            if (rootViewController != null) {
                rootViewController.presentViewController(picker, animated = true, completion = null)
            } else {
                Logger.e("CameraLauncher", "No root view controller available")
                pendingResult = null
                onResult(null)
            }
        } catch (e: Exception) {
            Logger.e("CameraLauncher", "Failed to launch picker: ${e.message}")
            pendingResult = null
            onResult(null)
        }
    }

    @OptIn(ExperimentalForeignApi::class)
    private fun handleImageResult(image: UIImage?) {
        val callback = pendingResult
        pendingResult = null
        imagePickerDelegate = null

        if (image == null) {
            callback?.invoke(null)
            return
        }

        try {
            // Convert UIImage to JPEG data
            val jpegData: NSData? = UIImageJPEGRepresentation(image, 0.8)
            if (jpegData == null) {
                Logger.e("CameraLauncher", "Failed to convert image to JPEG")
                callback?.invoke(null)
                return
            }

            // Convert NSData to ByteArray
            val bytes = jpegData.toByteArray()
            val fileName = "photo_${NSUUID().UUIDString}.jpg"

            callback?.invoke(CaptureResult(bytes, fileName, "image/jpeg"))
        } catch (e: Exception) {
            Logger.e("CameraLauncher", "Failed to process image: ${e.message}")
            callback?.invoke(null)
        }
    }

    @OptIn(ExperimentalForeignApi::class)
    private fun NSData.toByteArray(): ByteArray {
        val length = this.length.toInt()
        val bytes = ByteArray(length)
        if (length > 0) {
            kotlinx.cinterop.usePinned(bytes) { pinned ->
                platform.posix.memcpy(pinned.addressOf(0), this.bytes, this.length)
            }
        }
        return bytes
    }
}

/**
 * Delegate for UIImagePickerController callbacks.
 */
@OptIn(ExperimentalForeignApi::class)
private class ImagePickerDelegate(
    private val onImagePicked: (UIImage?) -> Unit
) : NSObject(), UIImagePickerControllerDelegateProtocol, UINavigationControllerDelegateProtocol {

    override fun imagePickerController(
        picker: UIImagePickerController,
        didFinishPickingMediaWithInfo: Map<Any?, *>
    ) {
        picker.dismissViewControllerAnimated(true) {
            val image = (didFinishPickingMediaWithInfo[UIImagePickerControllerEditedImage]
                ?: didFinishPickingMediaWithInfo[UIImagePickerControllerOriginalImage]) as? UIImage
            onImagePicked(image)
        }
    }

    override fun imagePickerControllerDidCancel(picker: UIImagePickerController) {
        picker.dismissViewControllerAnimated(true) {
            onImagePicked(null)
        }
    }
}
