package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.DataExportRequestDto
import com.logisticsx.driver.api.models.DataExportStatus
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.Chip
import com.logisticsx.driver.ui.components.DetailRow
import com.logisticsx.driver.ui.components.LoadingIndicator
import com.logisticsx.driver.ui.components.SectionCard
import com.logisticsx.driver.viewmodel.PrivacyViewModel
import com.logisticsx.driver.viewmodel.base.ActionState
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PrivacyScreen(
    onNavigateBack: () -> Unit,
    viewModel: PrivacyViewModel = koinViewModel(),
) {
    val isLoading by viewModel.isLoading.collectAsState()
    val exports by viewModel.exports.collectAsState()
    val deletions by viewModel.deletions.collectAsState()
    val exportAction by viewModel.exportAction.collectAsState()
    val deleteAction by viewModel.deleteAction.collectAsState()

    var showDeleteDialog by remember { mutableStateOf(false) }
    var deleteReason by remember { mutableStateOf("") }

    val pendingDeletion = deletions.firstOrNull { it.status?.value == "pending" }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Privacy",
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                },
            )
        },
    ) { paddingValues ->
        if (isLoading) {
            LoadingIndicator()
            return@Scaffold
        }

        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp),
        ) {
            SectionCard(title = "Download my data") {
                Text(
                    "We'll prepare a ZIP with your profile, organisations you have access to, " +
                        "consent history, and the activity records we hold for you. You'll get " +
                        "an email when it's ready (limit: one per 24 hours).",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )
                Spacer(Modifier.height(12.dp))
                Button(
                    onClick = { viewModel.requestExport() },
                    enabled = exportAction !is ActionState.Loading,
                    modifier = Modifier.fillMaxWidth(),
                ) { Text("Request export") }

                ActionFeedback(state = exportAction, successText = "Export requested.")

                if (exports.isNotEmpty()) {
                    Spacer(Modifier.height(12.dp))
                    HorizontalDivider()
                    Spacer(Modifier.height(12.dp))
                    exports.forEach { row -> ExportRow(row) }
                }
            }

            SectionCard(title = "Delete my account") {
                Text(
                    "Account deletion is irreversible after a 30-day grace period. Operational " +
                        "and financial records (loads, invoices, payments) stay intact for legal " +
                        "compliance — only your name, email, and phone are replaced with " +
                        "placeholders.",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )
                Spacer(Modifier.height(12.dp))

                if (pendingDeletion != null) {
                    DetailRow(
                        label = "Scheduled for",
                        value = pendingDeletion.scheduledFor?.toString() ?: "—",
                    )
                    Spacer(Modifier.height(12.dp))
                    if (pendingDeletion.isCancellable == true) {
                        OutlinedButton(
                            onClick = { viewModel.cancelDeletion(pendingDeletion.id ?: "") },
                            modifier = Modifier.fillMaxWidth(),
                        ) { Text("Cancel deletion") }
                    }
                } else {
                    OutlinedButton(
                        onClick = { showDeleteDialog = true; deleteReason = "" },
                        colors = ButtonDefaults.outlinedButtonColors(
                            contentColor = MaterialTheme.colorScheme.error,
                        ),
                        modifier = Modifier.fillMaxWidth(),
                    ) { Text("Delete my account") }
                }

                ActionFeedback(
                    state = deleteAction,
                    successText = "Deletion scheduled. You have 30 days to cancel.",
                )
            }
        }
    }

    if (showDeleteDialog) {
        AlertDialog(
            onDismissRequest = { showDeleteDialog = false },
            title = { Text("Delete my account") },
            text = {
                Column {
                    Text(
                        "This will schedule your personal data for permanent anonymization 30 " +
                            "days from now. Operational and financial records remain attached to " +
                            "your organization.",
                        style = MaterialTheme.typography.bodySmall,
                    )
                    Spacer(Modifier.height(12.dp))
                    OutlinedTextField(
                        value = deleteReason,
                        onValueChange = { deleteReason = it },
                        label = { Text("Reason (optional)") },
                        modifier = Modifier.fillMaxWidth(),
                    )
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        viewModel.requestDeletion(deleteReason.ifBlank { null })
                        showDeleteDialog = false
                    },
                    colors = ButtonDefaults.buttonColors(
                        containerColor = MaterialTheme.colorScheme.error,
                    ),
                ) { Text("Schedule deletion") }
            },
            dismissButton = {
                TextButton(onClick = { showDeleteDialog = false }) { Text("Cancel") }
            },
        )
    }
}

@Composable
private fun ExportRow(row: DataExportRequestDto) {
    Column(modifier = Modifier.padding(vertical = 6.dp)) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Text(
                row.requestedAt?.toString() ?: "—",
                style = MaterialTheme.typography.bodyMedium,
                modifier = Modifier.weight(1f),
            )
            ExportStatusChip(row.status)
        }
        if (row.status == DataExportStatus.READY) {
            Spacer(Modifier.height(4.dp))
            Text(
                "Download link is in the latest email; signed URLs expire after 1 hour.",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.primary,
            )
        }
        if (row.status == DataExportStatus.FAILED && !row.errorMessage.isNullOrBlank()) {
            Spacer(Modifier.height(4.dp))
            Text(
                row.errorMessage,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.error,
            )
        }
    }
}

@Composable
private fun ExportStatusChip(status: DataExportStatus?) {
    val color: Color = when (status) {
        DataExportStatus.READY -> MaterialTheme.colorScheme.primary
        DataExportStatus.FAILED -> MaterialTheme.colorScheme.error
        DataExportStatus.EXPIRED -> MaterialTheme.colorScheme.outline
        else -> MaterialTheme.colorScheme.tertiary
    }
    Chip(text = status?.value ?: "unknown", color = color)
}

@Composable
private fun ActionFeedback(state: ActionState<Unit>, successText: String) {
    when (state) {
        is ActionState.Success -> {
            Spacer(Modifier.height(8.dp))
            Text(
                successText,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.primary,
            )
        }
        is ActionState.Error -> {
            Spacer(Modifier.height(8.dp))
            Text(
                state.message,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.error,
            )
        }
        else -> {}
    }
}
