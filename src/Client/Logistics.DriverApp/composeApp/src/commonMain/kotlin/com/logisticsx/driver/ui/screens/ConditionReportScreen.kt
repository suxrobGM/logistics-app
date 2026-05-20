package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.InspectionType
import com.logisticsx.driver.api.models.LoadType
import com.logisticsx.driver.ui.components.SignaturePad
import com.logisticsx.driver.ui.components.capture.NotesTextField
import com.logisticsx.driver.ui.components.capture.PhotoCaptureSection
import com.logisticsx.driver.ui.components.capture.SubmitButton
import com.logisticsx.driver.ui.components.capture.rememberCameraCapture
import com.logisticsx.driver.ui.components.inspection.AddInspectionDefectDialog
import com.logisticsx.driver.ui.components.inspection.ContainerInputSection
import com.logisticsx.driver.ui.components.inspection.InspectionDefectView
import com.logisticsx.driver.ui.components.inspection.InspectionDefectsSection
import com.logisticsx.driver.ui.components.inspection.InspectionScreenScaffold
import com.logisticsx.driver.ui.components.inspection.VehicleInfoCard
import com.logisticsx.driver.ui.components.inspection.VinInputSection
import com.logisticsx.driver.util.BarcodeScannerLauncher
import com.logisticsx.driver.util.SignatureConverter
import com.logisticsx.driver.util.cargoPartCatalogGrouped
import com.logisticsx.driver.util.displayName
import com.logisticsx.driver.util.isContainerLoad
import com.logisticsx.driver.viewmodel.ConditionDefect
import com.logisticsx.driver.viewmodel.ConditionReportViewModel
import org.koin.compose.getKoin
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ConditionReportScreen(
    loadId: String,
    inspectionType: InspectionType,
    onNavigateBack: () -> Unit,
    viewModel: ConditionReportViewModel = koinViewModel { parametersOf(loadId, inspectionType) }
) {
    val onCapturePhoto = rememberCameraCapture(onPhotoCaptured = viewModel::addPhoto)
    val barcodeScannerLauncher: BarcodeScannerLauncher? = getKoin().getOrNull()

    val uiState by viewModel.uiState.collectAsState()
    var showAddDefectDialog by remember { mutableStateOf(false) }

    val title = inspectionType.displayTitle(uiState.cargoType)

    InspectionScreenScaffold(
        title = title,
        error = uiState.error,
        isSuccess = uiState.isSuccess,
        onClearError = viewModel::clearError,
        onNavigateBack = onNavigateBack
    ) {
        // Cargo-type-aware identifier block
        when {
            uiState.cargoType == LoadType.VEHICLE -> {
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
                    item { VehicleInfoCard(vehicleInfo = uiState.vehicleInfo!!) }
                }
            }

            uiState.cargoType.isContainerLoad -> {
                item {
                    ContainerInputSection(
                        containerNumber = uiState.containerNumber,
                        sealNumber = uiState.sealNumber,
                        onContainerNumberChange = viewModel::setContainerNumber,
                        onSealNumberChange = viewModel::setSealNumber,
                        onScanContainer = {
                            barcodeScannerLauncher?.launchScanner { result ->
                                result?.let { viewModel.setContainerNumber(it.value) }
                            }
                        }
                    )
                }
            }

            else -> {
                // Generic freight: no identifier section, just defects + photos + signature
            }
        }

        item {
            InspectionDefectsSection(
                defects = uiState.defects.map { it.toView() },
                onAddDefect = { showAddDefectDialog = true },
                onRemoveDefect = viewModel::removeDefect,
                sectionTitle = "Damage / Defects",
                addButtonText = "Add Damage"
            )
        }

        item {
            PhotoCaptureSection(
                photos = uiState.photos,
                onAddPhoto = onCapturePhoto ?: {},
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

    if (showAddDefectDialog) {
        AddInspectionDefectDialog(
            title = "Document Damage",
            groupedCategories = uiState.cargoType.cargoPartCatalogGrouped(),
            categoryDisplay = { it.displayName },
            onDismiss = { showAddDefectDialog = false },
            onConfirm = { partCategory, description, severity ->
                viewModel.addDefect(ConditionDefect(partCategory, description, severity))
                showAddDefectDialog = false
            }
        )
    }
}

private fun InspectionType.displayTitle(cargoType: LoadType): String {
    val action = when (this) {
        InspectionType.PICKUP -> "Pickup Inspection"
        InspectionType.DELIVERY -> "Delivery Inspection"
    }
    return when {
        cargoType == LoadType.VEHICLE -> "Vehicle $action"
        cargoType.isContainerLoad -> "Container $action"
        else -> action
    }
}

private fun ConditionDefect.toView(): InspectionDefectView = object : InspectionDefectView {
    override val categoryDisplay = partCategory.displayName
    override val description = this@toView.description
    override val severity = this@toView.severity
}
