package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.ui.components.capture.CaptureScreenScaffold
import com.logisticsx.driver.ui.components.capture.LocationDisplay
import com.logisticsx.driver.ui.components.capture.NotesTextField
import com.logisticsx.driver.ui.components.capture.PhotoCaptureSection
import com.logisticsx.driver.ui.components.capture.SignatureCaptureField
import com.logisticsx.driver.ui.components.capture.SubmitButton
import com.logisticsx.driver.ui.components.capture.rememberCameraCapture
import com.logisticsx.driver.viewmodel.DocumentCaptureType
import com.logisticsx.driver.viewmodel.PodCaptureViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PodCaptureScreen(
    onNavigateBack: () -> Unit,
    viewModel: PodCaptureViewModel
) {
    val onCapturePhoto = rememberCameraCapture(onPhotoCaptured = viewModel::addPhoto)
    val uiState by viewModel.uiState.collectAsState()

    val title = when (uiState.captureType) {
        DocumentCaptureType.POD -> "Capture Proof of Delivery"
        DocumentCaptureType.BOL -> "Capture Bill of Lading"
    }

    CaptureScreenScaffold(
        title = title,
        error = uiState.error,
        isSuccess = uiState.isSuccess,
        onClearError = viewModel::clearError,
        onNavigateBack = onNavigateBack
    ) {
        item {
            PhotoCaptureSection(
                photos = uiState.photos,
                onAddPhoto = onCapturePhoto,
                onRemovePhoto = viewModel::removePhoto,
                emptyMessage = "No photos added yet"
            )
        }

        item {
            SignatureCaptureField(
                onCaptured = viewModel::setSignature,
                onClear = viewModel::clearSignature
            )
            if (uiState.signaturePaths != null) {
                Text(
                    text = "Signature captured",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.padding(top = 4.dp)
                )
            }
        }

        item {
            OutlinedTextField(
                value = uiState.recipientName,
                onValueChange = viewModel::setRecipientName,
                label = { Text("Recipient Name") },
                placeholder = { Text("Enter recipient's name") },
                modifier = Modifier.fillMaxWidth(),
                singleLine = true
            )
        }

        item {
            NotesTextField(
                value = uiState.notes,
                onValueChange = viewModel::setNotes
            )
        }

        if (uiState.latitude != null && uiState.longitude != null) {
            item {
                LocationDisplay(
                    latitude = uiState.latitude!!,
                    longitude = uiState.longitude!!
                )
            }
        }

        item {
            SubmitButton(
                text = when (uiState.captureType) {
                    DocumentCaptureType.POD -> "Submit Proof of Delivery"
                    DocumentCaptureType.BOL -> "Submit Bill of Lading"
                },
                onClick = viewModel::submit,
                isLoading = uiState.isSubmitting,
                enabled = viewModel.canSubmit()
            )
        }
    }
}
