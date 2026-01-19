package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.InspectionType
import com.jfleets.driver.ui.components.SignaturePad
import com.jfleets.driver.ui.components.capture.NotesTextField
import com.jfleets.driver.ui.components.capture.PhotoCaptureSection
import com.jfleets.driver.ui.components.capture.SubmitButton
import com.jfleets.driver.ui.components.inspection.DamageMarkerDialog
import com.jfleets.driver.ui.components.inspection.DamageMarkersSection
import com.jfleets.driver.ui.components.inspection.VehicleInfoCard
import com.jfleets.driver.ui.components.inspection.VinInputSection
import com.jfleets.driver.util.BarcodeScannerLauncher
import com.jfleets.driver.util.CameraLauncher
import com.jfleets.driver.util.SignatureConverter
import com.jfleets.driver.viewmodel.CapturedPhoto
import com.jfleets.driver.viewmodel.ConditionReportViewModel
import org.koin.compose.getKoin
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf
import kotlin.uuid.ExperimentalUuidApi
import kotlin.uuid.Uuid

@OptIn(ExperimentalMaterial3Api::class, ExperimentalUuidApi::class)
@Composable
fun ConditionReportScreen(
    loadId: String,
    inspectionType: InspectionType,
    onNavigateBack: () -> Unit,
    viewModel: ConditionReportViewModel = koinViewModel { parametersOf(loadId, inspectionType) }
) {
    val cameraLauncher: CameraLauncher? = getKoin().getOrNull()
    val barcodeScannerLauncher: BarcodeScannerLauncher? = getKoin().getOrNull()

    val uiState by viewModel.uiState.collectAsState()
    val snackbarHostState = remember { SnackbarHostState() }
    var showDamageDialog by remember { mutableStateOf(false) }
    var pendingDamageX by remember { mutableStateOf(0.0) }
    var pendingDamageY by remember { mutableStateOf(0.0) }

    HandleSideEffects(
        error = uiState.error,
        isSuccess = uiState.isSuccess,
        snackbarHostState = snackbarHostState,
        onClearError = viewModel::clearError,
        onNavigateBack = onNavigateBack
    )

    val title = when (inspectionType) {
        InspectionType.PICKUP -> "Pickup Inspection"
        InspectionType.DELIVERY -> "Delivery Inspection"
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
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            item {
                VinInputSection(
                    vin = uiState.vin,
                    onVinChange = viewModel::setVin,
                    onScanVin = {
                        barcodeScannerLauncher?.launchScanner { result ->
                            result?.let { viewModel.setVin(it.value) }
                        }
                    },
                    onDecodeVin = viewModel::decodeVin,
                    isDecodingVin = uiState.isDecodingVin
                )
            }

            if (uiState.vehicleInfo != null) {
                item {
                    VehicleInfoCard(vehicleInfo = uiState.vehicleInfo!!)
                }
            }

            item {
                DamageMarkersSection(
                    damageMarkers = uiState.damageMarkers,
                    onDiagramTap = { x, y ->
                        pendingDamageX = x
                        pendingDamageY = y
                        showDamageDialog = true
                    },
                    onRemoveMarker = viewModel::removeDamageMarker
                )
            }

            item {
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
                    onRemovePhoto = viewModel::removePhoto
                )
            }

            item {
                SignaturePad(
                    modifier = Modifier.fillMaxWidth(),
                    onSignatureComplete = { paths ->
                        val base64 = SignatureConverter.pathsToBase64Png(paths) ?: ""
                        viewModel.setSignature(paths, base64)
                    },
                    onClear = viewModel::clearSignature
                )
            }

            item {
                NotesTextField(
                    value = uiState.notes,
                    onValueChange = viewModel::setNotes
                )
            }

            item {
                SubmitButton(
                    text = "Submit Condition Report",
                    onClick = viewModel::submit,
                    isLoading = uiState.isSubmitting,
                    enabled = viewModel.canSubmit()
                )

                Spacer(modifier = Modifier.height(16.dp))
            }
        }
    }

    if (showDamageDialog) {
        DamageMarkerDialog(
            onDismiss = { showDamageDialog = false },
            onConfirm = { description, severity ->
                viewModel.addDamageMarker(pendingDamageX, pendingDamageY, description, severity)
                showDamageDialog = false
            }
        )
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
