package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.ui.components.SignaturePad
import com.jfleets.driver.ui.components.capture.LocationDisplay
import com.jfleets.driver.ui.components.capture.NotesTextField
import com.jfleets.driver.ui.components.capture.PhotoCaptureSection
import com.jfleets.driver.ui.components.capture.SubmitButton
import com.jfleets.driver.util.CameraLauncher
import com.jfleets.driver.util.SignatureConverter
import com.jfleets.driver.viewmodel.CapturedPhoto
import com.jfleets.driver.viewmodel.DocumentCaptureType
import com.jfleets.driver.viewmodel.PodCaptureViewModel
import org.koin.compose.getKoin
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf
import kotlin.uuid.ExperimentalUuidApi
import kotlin.uuid.Uuid

@OptIn(ExperimentalMaterial3Api::class, ExperimentalUuidApi::class)
@Composable
fun PodCaptureScreen(
    loadId: String,
    tripStopId: String?,
    captureType: DocumentCaptureType,
    onNavigateBack: () -> Unit,
    viewModel: PodCaptureViewModel = koinViewModel { parametersOf(loadId, tripStopId, captureType) }
) {
    val cameraLauncher: CameraLauncher? = getKoin().getOrNull()
    val uiState by viewModel.uiState.collectAsState()
    val snackbarHostState = remember { SnackbarHostState() }

    HandleSideEffects(
        error = uiState.error,
        isSuccess = uiState.isSuccess,
        snackbarHostState = snackbarHostState,
        onClearError = viewModel::clearError,
        onNavigateBack = onNavigateBack
    )

    val title = when (captureType) {
        DocumentCaptureType.POD -> "Capture Proof of Delivery"
        DocumentCaptureType.BOL -> "Capture Bill of Lading"
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(title) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                    }
                }
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .verticalScroll(rememberScrollState())
                .padding(16.dp)
        ) {
            PhotoCaptureSection(
                photos = uiState.photos,
                onAddPhoto = {
                    cameraLauncher?.launchCamera { result ->
                        result?.let {
                            viewModel.addPhoto(
                                CapturedPhoto(
                                    id = Uuid.random().toString(),
                                    bytes = it.bytes,
                                    fileName = it.fileName,
                                    contentType = it.contentType
                                )
                            )
                        }
                    }
                },
                onRemovePhoto = viewModel::removePhoto,
                emptyMessage = "No photos added yet"
            )

            Spacer(modifier = Modifier.height(24.dp))

            SignaturePad(
                modifier = Modifier.fillMaxWidth(),
                onSignatureComplete = { paths ->
                    val base64 = SignatureConverter.pathsToBase64Png(paths) ?: ""
                    viewModel.setSignature(paths, base64)
                },
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

            Spacer(modifier = Modifier.height(24.dp))

            OutlinedTextField(
                value = uiState.recipientName,
                onValueChange = viewModel::setRecipientName,
                label = { Text("Recipient Name") },
                placeholder = { Text("Enter recipient's name") },
                modifier = Modifier.fillMaxWidth(),
                singleLine = true
            )

            Spacer(modifier = Modifier.height(16.dp))

            NotesTextField(
                value = uiState.notes,
                onValueChange = viewModel::setNotes
            )

            Spacer(modifier = Modifier.height(16.dp))

            if (uiState.latitude != null && uiState.longitude != null) {
                LocationDisplay(
                    latitude = uiState.latitude!!,
                    longitude = uiState.longitude!!
                )
            }

            Spacer(modifier = Modifier.height(24.dp))

            SubmitButton(
                text = when (captureType) {
                    DocumentCaptureType.POD -> "Submit Proof of Delivery"
                    DocumentCaptureType.BOL -> "Submit Bill of Lading"
                },
                onClick = viewModel::submit,
                isLoading = uiState.isSubmitting,
                enabled = viewModel.canSubmit()
            )

            Spacer(modifier = Modifier.height(16.dp))
        }
    }
}

@Composable
private fun HandleSideEffects(
    error: String?,
    isSuccess: Boolean,
    snackbarHostState: SnackbarHostState,
    onClearError: () -> Unit,
    onNavigateBack: () -> Unit
) {
    LaunchedEffect(error) {
        error?.let {
            snackbarHostState.showSnackbar(it)
            onClearError()
        }
    }

    LaunchedEffect(isSuccess) {
        if (isSuccess) {
            onNavigateBack()
        }
    }
}
