package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.DvirInspectionCategory
import com.logisticsx.driver.api.models.DvirType
import com.logisticsx.driver.ui.components.SectionCard
import com.logisticsx.driver.ui.components.capture.CaptureScreenScaffold
import com.logisticsx.driver.ui.components.capture.NotesTextField
import com.logisticsx.driver.ui.components.capture.PhotoCaptureSection
import com.logisticsx.driver.ui.components.capture.SignatureCaptureField
import com.logisticsx.driver.ui.components.capture.SubmitButton
import com.logisticsx.driver.ui.components.capture.rememberCameraCapture
import com.logisticsx.driver.ui.components.dvir.DvirInspectionTypeSelector
import com.logisticsx.driver.ui.components.dvir.DvirTruckSelector
import com.logisticsx.driver.ui.components.inspection.AddInspectionDefectDialog
import com.logisticsx.driver.ui.components.inspection.InspectionDefectView
import com.logisticsx.driver.ui.components.inspection.InspectionDefectsSection
import com.logisticsx.driver.util.displayName
import com.logisticsx.driver.util.grouped
import com.logisticsx.driver.viewmodel.DvirDefect
import com.logisticsx.driver.viewmodel.DvirFormViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DvirFormScreen(
    onNavigateBack: () -> Unit,
    viewModel: DvirFormViewModel
) {
    val onCapturePhoto = rememberCameraCapture(onPhotoCaptured = viewModel::addPhoto)
    val uiState by viewModel.uiState.collectAsState()
    var showAddDefectDialog by remember { mutableStateOf(false) }

    val title = when (uiState.dvirType) {
        DvirType.PRE_TRIP -> "Pre-Trip Inspection"
        DvirType.POST_TRIP -> "Post-Trip Inspection"
    }

    CaptureScreenScaffold(
        title = title,
        error = uiState.error,
        isSuccess = uiState.isSuccess,
        onClearError = viewModel::clearError,
        onNavigateBack = onNavigateBack
    ) {
        item {
            DvirInspectionTypeSelector(
                selectedType = uiState.dvirType,
                onTypeSelected = viewModel::setDvirType
            )
        }

        item {
            DvirTruckSelector(
                selectedTruck = uiState.selectedTruck,
                availableTrucks = uiState.availableTrucks,
                isLoading = uiState.isLoadingTrucks,
                onTruckSelected = viewModel::selectTruck,
                enabled = uiState.truckId == null
            )
        }

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

        item {
            InspectionDefectsSection(
                defects = uiState.defects.map {
                    InspectionDefectView(it.category.displayName, it.description, it.severity)
                },
                onAddDefect = { showAddDefectDialog = true },
                onRemoveDefect = viewModel::removeDefect
            )
        }

        item {
            PhotoCaptureSection(
                photos = uiState.photos,
                onAddPhoto = onCapturePhoto,
                onRemovePhoto = viewModel::removePhoto
            )
        }

        item {
            SignatureCaptureField(
                onCaptured = viewModel::setSignature,
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
                text = "Submit DVIR",
                onClick = viewModel::submit,
                isLoading = uiState.isSubmitting,
                enabled = viewModel.canSubmit()
            )
            Spacer(modifier = Modifier.height(16.dp))
        }
    }

    if (showAddDefectDialog) {
        AddInspectionDefectDialog(
            title = "Report Defect",
            groupedCategories = DvirInspectionCategory.grouped,
            categoryDisplay = { it.displayName },
            onDismiss = { showAddDefectDialog = false },
            onConfirm = { category, description, severity ->
                viewModel.addDefect(DvirDefect(category, description, severity))
                showAddDefectDialog = false
            }
        )
    }
}
