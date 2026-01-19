package com.jfleets.driver.util

import android.Manifest
import android.content.pm.PackageManager
import androidx.activity.ComponentActivity
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.contract.ActivityResultContracts
import androidx.annotation.OptIn
import androidx.camera.core.CameraSelector
import androidx.camera.core.ExperimentalGetImage
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.ImageProxy
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.core.content.ContextCompat
import com.google.mlkit.vision.barcode.BarcodeScannerOptions
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.barcode.common.Barcode
import com.google.mlkit.vision.common.InputImage
import java.util.concurrent.ExecutorService
import java.util.concurrent.Executors

/**
 * Android implementation using ML Kit Barcode Scanning with CameraX.
 */
actual class BarcodeScannerLauncher(
    private val activity: ComponentActivity
) {
    private var pendingResult: ((ScanResult?) -> Unit)? = null
    private var cameraExecutor: ExecutorService? = null
    private var scannerDialog: BarcodeScannerDialog? = null

    private val permissionLauncher: ActivityResultLauncher<String> =
        activity.registerForActivityResult(ActivityResultContracts.RequestPermission()) { granted ->
            if (granted) {
                startScanning()
            } else {
                Logger.w("BarcodeScannerLauncher", "Camera permission denied")
                val callback = pendingResult
                pendingResult = null
                callback?.invoke(null)
            }
        }

    actual fun launchScanner(onResult: (ScanResult?) -> Unit) {
        pendingResult = onResult

        // Check camera permission
        if (ContextCompat.checkSelfPermission(activity, Manifest.permission.CAMERA)
            == PackageManager.PERMISSION_GRANTED
        ) {
            startScanning()
        } else {
            permissionLauncher.launch(Manifest.permission.CAMERA)
        }
    }

    private fun startScanning() {
        cameraExecutor = Executors.newSingleThreadExecutor()

        scannerDialog = BarcodeScannerDialog(
            activity = activity,
            onBarcodeDetected = { barcode ->
                handleBarcodeResult(barcode)
            },
            onDismiss = {
                val callback = pendingResult
                pendingResult = null
                cleanup()
                callback?.invoke(null)
            },
            cameraExecutor = cameraExecutor!!
        )

        scannerDialog?.show()
    }

    private fun handleBarcodeResult(barcode: Barcode) {
        val callback = pendingResult
        pendingResult = null

        scannerDialog?.dismiss()
        cleanup()

        val format = when (barcode.format) {
            Barcode.FORMAT_CODE_39 -> BarcodeFormat.CODE_39
            Barcode.FORMAT_CODE_128 -> BarcodeFormat.CODE_128
            Barcode.FORMAT_DATA_MATRIX -> BarcodeFormat.DATA_MATRIX
            Barcode.FORMAT_QR_CODE -> BarcodeFormat.QR_CODE
            else -> BarcodeFormat.UNKNOWN
        }

        val value = barcode.rawValue ?: barcode.displayValue ?: ""
        callback?.invoke(ScanResult(value, format))
    }

    private fun cleanup() {
        cameraExecutor?.shutdown()
        cameraExecutor = null
        scannerDialog = null
    }
}

/**
 * Dialog for displaying barcode scanner preview.
 */
private class BarcodeScannerDialog(
    private val activity: ComponentActivity,
    private val onBarcodeDetected: (Barcode) -> Unit,
    private val onDismiss: () -> Unit,
    private val cameraExecutor: ExecutorService
) {
    private var dialog: android.app.AlertDialog? = null
    private var cameraProvider: ProcessCameraProvider? = null

    @OptIn(ExperimentalGetImage::class)
    fun show() {
        val previewView = PreviewView(activity).apply {
            layoutParams = android.view.ViewGroup.LayoutParams(
                android.view.ViewGroup.LayoutParams.MATCH_PARENT,
                600
            )
        }

        dialog = android.app.AlertDialog.Builder(activity)
            .setTitle("Scan VIN Barcode")
            .setView(previewView)
            .setNegativeButton("Cancel") { _, _ ->
                dismiss()
                onDismiss()
            }
            .setOnCancelListener {
                dismiss()
                onDismiss()
            }
            .create()

        dialog?.show()

        // Start camera
        val cameraProviderFuture = ProcessCameraProvider.getInstance(activity)
        cameraProviderFuture.addListener({
            cameraProvider = cameraProviderFuture.get()
            bindCameraPreview(previewView)
        }, ContextCompat.getMainExecutor(activity))
    }

    @OptIn(ExperimentalGetImage::class)
    private fun bindCameraPreview(previewView: PreviewView) {
        val provider = cameraProvider ?: return

        val preview = Preview.Builder()
            .build()
            .also {
                it.surfaceProvider = previewView.surfaceProvider
            }

        val options = BarcodeScannerOptions.Builder()
            .setBarcodeFormats(
                Barcode.FORMAT_CODE_39,
                Barcode.FORMAT_CODE_128,
                Barcode.FORMAT_DATA_MATRIX,
                Barcode.FORMAT_QR_CODE
            )
            .build()

        val scanner = BarcodeScanning.getClient(options)

        val imageAnalysis = ImageAnalysis.Builder()
            .setBackpressureStrategy(ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST)
            .build()
            .also {
                it.setAnalyzer(cameraExecutor) { imageProxy ->
                    processImage(imageProxy, scanner)
                }
            }

        try {
            provider.unbindAll()
            provider.bindToLifecycle(
                activity,
                CameraSelector.DEFAULT_BACK_CAMERA,
                preview,
                imageAnalysis
            )
        } catch (e: Exception) {
            Logger.e("BarcodeScannerDialog", "Failed to bind camera", e)
        }
    }

    @OptIn(ExperimentalGetImage::class)
    private fun processImage(imageProxy: ImageProxy, scanner: com.google.mlkit.vision.barcode.BarcodeScanner) {
        val mediaImage = imageProxy.image
        if (mediaImage == null) {
            imageProxy.close()
            return
        }

        val image = InputImage.fromMediaImage(mediaImage, imageProxy.imageInfo.rotationDegrees)

        scanner.process(image)
            .addOnSuccessListener { barcodes ->
                if (barcodes.isNotEmpty()) {
                    // Take the first detected barcode
                    val barcode = barcodes.first()
                    onBarcodeDetected(barcode)
                }
            }
            .addOnFailureListener { e ->
                Logger.e("BarcodeScannerDialog", "Barcode scanning failed", e)
            }
            .addOnCompleteListener {
                imageProxy.close()
            }
    }

    fun dismiss() {
        try {
            cameraProvider?.unbindAll()
            dialog?.dismiss()
        } catch (e: Exception) {
            Logger.e("BarcodeScannerDialog", "Error dismissing dialog", e)
        }
    }
}
