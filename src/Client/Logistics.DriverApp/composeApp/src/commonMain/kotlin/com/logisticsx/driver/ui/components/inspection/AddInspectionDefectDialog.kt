package com.logisticsx.driver.ui.components.inspection

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
import com.logisticsx.driver.api.models.DefectSeverity
import com.logisticsx.driver.util.displayName

/**
 * Generic "add defect" dialog used by both DVIR and cargo Condition Report.
 *
 * The caller supplies:
 *  - [groupedCategories]: section name -> list of category values valid for
 *    the inspection type (DVIR uses [com.logisticsx.driver.api.models.DvirInspectionCategory.Companion.grouped],
 *    Condition Report uses [com.logisticsx.driver.util.cargoPartCatalogGrouped]).
 *  - [categoryDisplay]: how to render a single category value as a chip label.
 *  - [onConfirm]: called with the chosen category, description, and severity.
 */
@OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
@Composable
fun <T : Any> AddInspectionDefectDialog(
    title: String,
    groupedCategories: Map<String, List<T>>,
    categoryDisplay: (T) -> String,
    onDismiss: () -> Unit,
    onConfirm: (category: T, description: String, severity: DefectSeverity) -> Unit
) {
    var selectedCategory by remember { mutableStateOf<T?>(null) }
    var description by remember { mutableStateOf("") }
    var severity by remember { mutableStateOf(DefectSeverity.MINOR) }
    var expandedGroup by remember { mutableStateOf<String?>(null) }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(title) },
        text = {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                item {
                    Text(
                        text = "Category",
                        style = MaterialTheme.typography.labelLarge
                    )
                }

                groupedCategories.forEach { (groupName, categories) ->
                    item {
                        CategoryGroupCard(
                            groupName = groupName,
                            categories = categories,
                            selectedCategory = selectedCategory,
                            categoryDisplay = categoryDisplay,
                            isExpanded = expandedGroup == groupName,
                            onExpandToggle = {
                                expandedGroup = if (expandedGroup == groupName) null else groupName
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
                        onConfirm(cat, description, severity)
                    }
                },
                enabled = selectedCategory != null && description.isNotBlank()
            ) {
                Text("Add")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) { Text("Cancel") }
        }
    )
}

@OptIn(ExperimentalLayoutApi::class)
@Composable
private fun <T : Any> CategoryGroupCard(
    groupName: String,
    categories: List<T>,
    selectedCategory: T?,
    categoryDisplay: (T) -> String,
    isExpanded: Boolean,
    onExpandToggle: () -> Unit,
    onCategorySelected: (T) -> Unit
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
                            label = { Text(categoryDisplay(category)) }
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
        Text(text = "Severity", style = MaterialTheme.typography.labelLarge)
        Spacer(modifier = Modifier.height(8.dp))
        FlowRow(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
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
