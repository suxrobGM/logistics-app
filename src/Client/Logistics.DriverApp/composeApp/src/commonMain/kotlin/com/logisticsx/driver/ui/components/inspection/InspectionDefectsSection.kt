package com.logisticsx.driver.ui.components.inspection

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.DefectSeverity
import com.logisticsx.driver.ui.components.CardContainer
import com.logisticsx.driver.util.displayName

/**
 * Minimal interface every defect kind in the app must satisfy so the shared
 * inspection components can render it. Both [com.logisticsx.driver.viewmodel.DvirDefect]
 * (truck DVIR) and [com.logisticsx.driver.viewmodel.ConditionDefect] (cargo
 * inspection) implement this via adapter functions.
 */
interface InspectionDefectView {
    val categoryDisplay: String
    val description: String
    val severity: DefectSeverity
}

/**
 * Adapts any defect kind (DVIR / cargo condition) into an [InspectionDefectView]
 * for the shared inspection components, so screens don't each define their own adapter.
 */
fun InspectionDefectView(
    categoryDisplay: String,
    description: String,
    severity: DefectSeverity
): InspectionDefectView = object : InspectionDefectView {
    override val categoryDisplay = categoryDisplay
    override val description = description
    override val severity = severity
}

/**
 * Generalized defects section — used by both the DVIR form and the cargo
 * Condition Report. The owner screen passes in already-adapted defects, an
 * optional [sectionTitle], and the empty-state hint.
 */
@Composable
fun InspectionDefectsSection(
    defects: List<InspectionDefectView>,
    onAddDefect: () -> Unit,
    onRemoveDefect: (Int) -> Unit,
    modifier: Modifier = Modifier,
    sectionTitle: String = "Defects Found",
    addButtonText: String = "Add Defect",
    emptyStateText: String = "No defects reported. Tap 'Add Defect' if you find any issues."
) {
    CardContainer(modifier = modifier) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = sectionTitle,
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold
                )
                TextButton(onClick = onAddDefect) {
                    Icon(Icons.Filled.Add, contentDescription = null)
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(addButtonText)
                }
            }

            if (defects.isEmpty()) {
                Text(
                    text = emptyStateText,
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.padding(vertical = 8.dp)
                )
            } else {
                Spacer(modifier = Modifier.size(8.dp))
                defects.forEachIndexed { index, defect ->
                    DefectItem(defect = defect, onRemove = { onRemoveDefect(index) })
                    if (index < defects.size - 1) {
                        HorizontalDivider(modifier = Modifier.padding(vertical = 8.dp))
                    }
                }
            }
        }
    }
}

@Composable
private fun DefectItem(defect: InspectionDefectView, onRemove: () -> Unit) {
    Row(modifier = Modifier.fillMaxWidth(), verticalAlignment = Alignment.Top) {
        Icon(
            imageVector = Icons.Filled.Warning,
            contentDescription = null,
            tint = severityColor(defect.severity),
            modifier = Modifier.size(20.dp)
        )
        Spacer(modifier = Modifier.width(8.dp))
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = defect.categoryDisplay,
                style = MaterialTheme.typography.bodyMedium,
                fontWeight = FontWeight.Bold
            )
            Text(text = defect.description, style = MaterialTheme.typography.bodySmall)
            Text(
                text = defect.severity.displayName,
                style = MaterialTheme.typography.labelSmall,
                color = severityColor(defect.severity)
            )
        }
        IconButton(onClick = onRemove) {
            Icon(Icons.Filled.Close, contentDescription = "Remove")
        }
    }
}

@Composable
private fun severityColor(severity: DefectSeverity) = when (severity) {
    DefectSeverity.OUT_OF_SERVICE -> MaterialTheme.colorScheme.error
    DefectSeverity.MAJOR -> MaterialTheme.colorScheme.tertiary
    DefectSeverity.MINOR -> MaterialTheme.colorScheme.secondary
}
