package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
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
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.DvirType
import com.jfleets.driver.ui.components.SectionCard
import com.jfleets.driver.ui.components.SignaturePad
import com.jfleets.driver.ui.components.capture.NotesTextField
import com.jfleets.driver.ui.components.capture.PhotoCaptureSection
import com.jfleets.driver.ui.components.capture.SubmitButton
import com.jfleets.driver.ui.screens.dvir.DvirAddDefectDialog
import com.jfleets.driver.ui.screens.dvir.DvirDefectsSection
import com.jfleets.driver.ui.screens.dvir.DvirInspectionTypeSelector
import com.jfleets.driver.ui.screens.dvir.DvirTruckSelector
import com.jfleets.driver.util.CameraLauncher
import com.jfleets.driver.util.SignatureConverter
import com.jfleets.driver.viewmodel.CapturedPhoto
import com.jfleets.driver.viewmodel.DvirFormViewModel
import org.koin.compose.getKoin
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf
import kotlin.uuid.ExperimentalUuidApi
import kotlin.uuid.Uuid

@OptIn(ExperimentalMaterial3Api::class, ExperimentalUuidApi::class)
@Composable
fun DvirFormScreen(
    truckId: String?,
    tripId: String?,
    onNavigateBack: () -> Unit,
    viewModel: DvirFormViewModel = koinViewModel { parametersOf(truckId, tripId) }
) {
    val cameraLauncher: CameraLauncher? = getKoin().getOrNull()
    val uiState by viewModel.uiState.collectAsState()
    val snackbarHostState = remember { SnackbarHostState() }
    var showAddDefectDialog by remember { mutableStateOf(false) }

    HandleSideEffects(
        error = uiState.error,
        isSuccess = uiState.isSuccess,
        snackbarHostState = snackbarHostState,
        onClearError = viewModel::clearError,
        onNavigateBack = onNavigateBack
    )

    val title = when (uiState.dvirType) {
        DvirType.PRE_TRIP -> "Pre-Trip Inspection"
        DvirType.POST_TRIP -> "Post-Trip Inspection"
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
            // Inspection Type Selection
            item {
                DvirInspectionTypeSelector(
                    selectedType = uiState.dvirType,
                    onTypeSelected = viewModel::setDvirType
                )
            }

            // Truck Selection
            item {
                DvirTruckSelector(
                    selectedTruck = uiState.selectedTruck,
                    availableTrucks = uiState.availableTrucks,
                    isLoading = uiState.isLoadingTrucks,
                    onTruckSelected = viewModel::selectTruck
                )
            }

            // Odometer Reading
            item {
                SectionCard(title = "Odometer") {
                    OutlinedTextField(
                        value = uiState.odometerReading,
                        onValueChange = viewModel::setOdometerReading,
                        label = { Text("Odometer Reading") },
                        placeholder = { Text("Enter current odometer") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        modifier = Modifier.fillMaxWidth(),
                        suffix = { Text("miles") }
                    )
                }
            }

            // Defects Section
            item {
                DvirDefectsSection(
                    defects = uiState.defects,
                    onAddDefect = { showAddDefectDialog = true },
                    onRemoveDefect = viewModel::removeDefect
                )
            }

            // Photos
            item {
                SectionCard(title = "Photos") {
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
                        showTitle = false
                    )
                }
            }

            // Driver Signature
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

            // Notes
            item {
                NotesTextField(
                    value = uiState.notes,
                    onValueChange = viewModel::setNotes
                )
            }

            // Submit Button
            item {
                SubmitButton(
                    text = "Submit DVIR",
                    onClick = viewModel::submit,
                    isLoading = uiState.isSubmitting,
                    enabled = viewModel.canSubmit()
                )

                Spacer(modifier = Modifier.height(16.dp))
            }
        }
    }

    if (showAddDefectDialog) {
        DvirAddDefectDialog(
            onDismiss = { showAddDefectDialog = false },
            onConfirm = { defect ->
                viewModel.addDefect(defect)
                showAddDefectDialog = false
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
