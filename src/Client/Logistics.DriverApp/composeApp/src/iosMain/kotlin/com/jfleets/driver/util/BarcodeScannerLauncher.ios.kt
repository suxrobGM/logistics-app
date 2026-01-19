package com.jfleets.driver.util

import kotlinx.cinterop.ExperimentalForeignApi
import platform.AVFoundation.AVAuthorizationStatusAuthorized
import platform.AVFoundation.AVAuthorizationStatusNotDetermined
import platform.AVFoundation.AVCaptureDevice
import platform.AVFoundation.AVCaptureDeviceInput
import platform.AVFoundation.AVCaptureMetadataOutput
import platform.AVFoundation.AVCaptureMetadataOutputObjectsDelegateProtocol
import platform.AVFoundation.AVCaptureSession
import platform.AVFoundation.AVCaptureSessionPresetHigh
import platform.AVFoundation.AVCaptureVideoPreviewLayer
import platform.AVFoundation.AVLayerVideoGravityResizeAspectFill
import platform.AVFoundation.AVMediaTypeVideo
import platform.AVFoundation.AVMetadataMachineReadableCodeObject
import platform.AVFoundation.AVMetadataObjectTypeCode128Code
import platform.AVFoundation.AVMetadataObjectTypeCode39Code
import platform.AVFoundation.AVMetadataObjectTypeDataMatrixCode
import platform.AVFoundation.AVMetadataObjectTypeQRCode
import platform.AVFoundation.authorizationStatusForMediaType
import platform.AVFoundation.requestAccessForMediaType
import platform.CoreGraphics.CGRectMake
import platform.UIKit.UIAlertAction
import platform.UIKit.UIAlertActionStyleCancel
import platform.UIKit.UIAlertController
import platform.UIKit.UIAlertControllerStyleAlert
import platform.UIKit.UIApplication
import platform.UIKit.UIColor
import platform.UIKit.UIView
import platform.UIKit.UIViewController
import platform.darwin.NSObject
import platform.darwin.dispatch_get_main_queue
import platform.darwin.dispatch_async

/**
 * iOS implementation using AVFoundation barcode scanning.
 */
actual class BarcodeScannerLauncher {
    private var pendingResult: ((ScanResult?) -> Unit)? = null
    private var scannerViewController: BarcodeScannerViewController? = null

    actual fun launchScanner(onResult: (ScanResult?) -> Unit) {
        pendingResult = onResult

        // Check camera permission
        val status = AVCaptureDevice.authorizationStatusForMediaType(AVMediaTypeVideo)

        when (status) {
            AVAuthorizationStatusAuthorized -> {
                showScanner()
            }
            AVAuthorizationStatusNotDetermined -> {
                AVCaptureDevice.requestAccessForMediaType(AVMediaTypeVideo) { granted ->
                    dispatch_async(dispatch_get_main_queue()) {
                        if (granted) {
                            showScanner()
                        } else {
                            Logger.w("BarcodeScannerLauncher", "Camera permission denied")
                            val callback = pendingResult
                            pendingResult = null
                            callback?.invoke(null)
                        }
                    }
                }
            }
            else -> {
                Logger.w("BarcodeScannerLauncher", "Camera permission not available")
                pendingResult = null
                onResult(null)
            }
        }
    }

    @OptIn(ExperimentalForeignApi::class)
    private fun showScanner() {
        val rootViewController = UIApplication.sharedApplication.keyWindow?.rootViewController
        if (rootViewController == null) {
            Logger.e("BarcodeScannerLauncher", "No root view controller")
            val callback = pendingResult
            pendingResult = null
            callback?.invoke(null)
            return
        }

        scannerViewController = BarcodeScannerViewController(
            onBarcodeDetected = { value, format ->
                handleResult(value, format)
            },
            onDismiss = {
                val callback = pendingResult
                pendingResult = null
                scannerViewController = null
                callback?.invoke(null)
            }
        )

        rootViewController.presentViewController(
            scannerViewController!!,
            animated = true,
            completion = null
        )
    }

    private fun handleResult(value: String, format: BarcodeFormat) {
        val callback = pendingResult
        pendingResult = null

        scannerViewController?.dismissViewControllerAnimated(true) {
            scannerViewController = null
            callback?.invoke(ScanResult(value, format))
        }
    }
}

/**
 * View controller for barcode scanning using AVFoundation.
 */
@OptIn(ExperimentalForeignApi::class)
private class BarcodeScannerViewController(
    private val onBarcodeDetected: (String, BarcodeFormat) -> Unit,
    private val onDismiss: () -> Unit
) : UIViewController(nibName = null, bundle = null), AVCaptureMetadataOutputObjectsDelegateProtocol {

    private var captureSession: AVCaptureSession? = null
    private var previewLayer: AVCaptureVideoPreviewLayer? = null
    private var hasFoundBarcode = false

    override fun viewDidLoad() {
        super.viewDidLoad()

        view.backgroundColor = UIColor.blackColor

        setupCamera()
        setupCancelButton()
    }

    override fun viewWillAppear(animated: Boolean) {
        super.viewWillAppear(animated)

        if (captureSession?.isRunning() == false) {
            dispatch_async(dispatch_get_main_queue()) {
                captureSession?.startRunning()
            }
        }
    }

    override fun viewWillDisappear(animated: Boolean) {
        super.viewWillDisappear(animated)

        if (captureSession?.isRunning() == true) {
            captureSession?.stopRunning()
        }
    }

    private fun setupCamera() {
        captureSession = AVCaptureSession()
        captureSession?.sessionPreset = AVCaptureSessionPresetHigh

        val device = AVCaptureDevice.defaultDeviceWithMediaType(AVMediaTypeVideo)
        if (device == null) {
            Logger.e("BarcodeScannerViewController", "No camera device available")
            showError("Camera not available")
            return
        }

        try {
            val input = AVCaptureDeviceInput.deviceInputWithDevice(device, null)
            if (input != null && captureSession?.canAddInput(input) == true) {
                captureSession?.addInput(input)
            }

            val output = AVCaptureMetadataOutput()
            if (captureSession?.canAddOutput(output) == true) {
                captureSession?.addOutput(output)

                output.setMetadataObjectsDelegate(this, dispatch_get_main_queue())
                output.metadataObjectTypes = listOf(
                    AVMetadataObjectTypeCode39Code,
                    AVMetadataObjectTypeCode128Code,
                    AVMetadataObjectTypeDataMatrixCode,
                    AVMetadataObjectTypeQRCode
                )
            }

            previewLayer = AVCaptureVideoPreviewLayer(session = captureSession!!)
            previewLayer?.videoGravity = AVLayerVideoGravityResizeAspectFill
            previewLayer?.frame = view.bounds
            view.layer.addSublayer(previewLayer!!)

        } catch (e: Exception) {
            Logger.e("BarcodeScannerViewController", "Failed to setup camera: ${e.message}")
            showError("Failed to start camera")
        }
    }

    private fun setupCancelButton() {
        val alert = UIAlertController.alertControllerWithTitle(
            title = "Scan VIN Barcode",
            message = "Point camera at barcode",
            preferredStyle = UIAlertControllerStyleAlert
        )

        // Note: In a real implementation, you'd add a cancel button overlay to the camera view
        // For simplicity, we'll add a gesture recognizer or navigation bar
    }

    private fun showError(message: String) {
        val alert = UIAlertController.alertControllerWithTitle(
            title = "Error",
            message = message,
            preferredStyle = UIAlertControllerStyleAlert
        )
        alert.addAction(
            UIAlertAction.actionWithTitle("OK", style = UIAlertActionStyleCancel) { _ ->
                dismissViewControllerAnimated(true) {
                    onDismiss()
                }
            }
        )
        presentViewController(alert, animated = true, completion = null)
    }

    override fun captureOutput(
        output: platform.AVFoundation.AVCaptureOutput,
        didOutputMetadataObjects: List<*>,
        fromConnection: platform.AVFoundation.AVCaptureConnection
    ) {
        if (hasFoundBarcode) return

        val metadataObjects = didOutputMetadataObjects.filterIsInstance<AVMetadataMachineReadableCodeObject>()
        if (metadataObjects.isEmpty()) return

        val barcode = metadataObjects.first()
        val value = barcode.stringValue ?: return

        hasFoundBarcode = true
        captureSession?.stopRunning()

        val format = when (barcode.type) {
            AVMetadataObjectTypeCode39Code -> BarcodeFormat.CODE_39
            AVMetadataObjectTypeCode128Code -> BarcodeFormat.CODE_128
            AVMetadataObjectTypeDataMatrixCode -> BarcodeFormat.DATA_MATRIX
            AVMetadataObjectTypeQRCode -> BarcodeFormat.QR_CODE
            else -> BarcodeFormat.UNKNOWN
        }

        onBarcodeDetected(value, format)
    }

    override fun touchesBegan(touches: Set<*>, withEvent: platform.UIKit.UIEvent?) {
        super.touchesBegan(touches, withEvent)
        // Tap to dismiss
        dismissViewControllerAnimated(true) {
            onDismiss()
        }
    }
}
