package com.jfleets.driver.ui.screens.dvir

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FilterChip
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.DefectSeverity
import com.jfleets.driver.api.models.DvirInspectionCategory
import com.jfleets.driver.util.displayName
import com.jfleets.driver.util.grouped
import com.jfleets.driver.viewmodel.DvirDefect

@OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
@Composable
fun DvirAddDefectDialog(
    onDismiss: () -> Unit,
    onConfirm: (DvirDefect) -> Unit
) {
    var selectedCategory by remember { mutableStateOf<DvirInspectionCategory?>(null) }
    var description by remember { mutableStateOf("") }
    var severity by remember { mutableStateOf(DefectSeverity.MINOR) }
    var expandedCategoryGroup by remember { mutableStateOf<String?>(null) }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Report Defect") },
        text = {
            LazyColumn(
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                item {
                    Text(
                        text = "Category",
                        style = MaterialTheme.typography.labelLarge
                    )
                }

                // Category Groups
                DvirInspectionCategory.grouped.forEach { (groupName, categories) ->
                    item {
                        CategoryGroupCard(
                            groupName = groupName,
                            categories = categories,
                            selectedCategory = selectedCategory,
                            isExpanded = expandedCategoryGroup == groupName,
                            onExpandToggle = {
                                expandedCategoryGroup =
                                    if (expandedCategoryGroup == groupName) null else groupName
                            },
                            onCategorySelected = { selectedCategory = it }
                        )
                    }
                }

                item {
                    OutlinedTextField(
                        value = description,
                        onValueChange = { description = it },
                        label = { Text("Description") },
                        placeholder = { Text("Describe the defect") },
                        modifier = Modifier.fillMaxWidth(),
                        minLines = 2,
                        maxLines = 4
                    )
                }

                item {
                    SeveritySelector(
                        selectedSeverity = severity,
                        onSeveritySelected = { severity = it }
                    )
                }
            }
        },
        confirmButton = {
            TextButton(
                onClick = {
                    selectedCategory?.let { cat ->
                        onConfirm(
                            DvirDefect(
                                category = cat,
                                description = description,
                                severity = severity
                            )
                        )
                    }
                },
                enabled = selectedCategory != null && description.isNotBlank()
            ) {
                Text("Add")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    )
}

@OptIn(ExperimentalLayoutApi::class)
@Composable
private fun CategoryGroupCard(
    groupName: String,
    categories: List<DvirInspectionCategory>,
    selectedCategory: DvirInspectionCategory?,
    isExpanded: Boolean,
    onExpandToggle: () -> Unit,
    onCategorySelected: (DvirInspectionCategory) -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable(onClick = onExpandToggle),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Text(
                text = groupName,
                style = MaterialTheme.typography.titleSmall,
                fontWeight = FontWeight.Bold
            )
            if (isExpanded) {
                Spacer(modifier = Modifier.height(8.dp))
                FlowRow(
                    horizontalArrangement = Arrangement.spacedBy(4.dp),
                    verticalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    categories.forEach { category ->
                        FilterChip(
                            selected = selectedCategory == category,
                            onClick = { onCategorySelected(category) },
                            label = { Text(category.displayName) }
                        )
                    }
                }
            }
        }
    }
}

@OptIn(ExperimentalLayoutApi::class)
@Composable
private fun SeveritySelector(
    selectedSeverity: DefectSeverity,
    onSeveritySelected: (DefectSeverity) -> Unit
) {
    Column {
        Text(
            text = "Severity",
            style = MaterialTheme.typography.labelLarge
        )
        Spacer(modifier = Modifier.height(8.dp))
        FlowRow(
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            DefectSeverity.entries.forEach { sev ->
                FilterChip(
                    selected = selectedSeverity == sev,
                    onClick = { onSeveritySelected(sev) },
                    label = { Text(sev.displayName) },
                    border = BorderStroke(
                        1.dp,
                        when (sev) {
                            DefectSeverity.OUT_OF_SERVICE -> MaterialTheme.colorScheme.error
                            DefectSeverity.MAJOR -> MaterialTheme.colorScheme.tertiary
                            DefectSeverity.MINOR -> MaterialTheme.colorScheme.secondary
                        }
                    )
                )
            }
        }
    }
}
