package com.jfleets.driver.ui.screens.dvir

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
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
import com.jfleets.driver.api.models.DefectSeverity
import com.jfleets.driver.ui.components.CardContainer
import com.jfleets.driver.util.displayName
import com.jfleets.driver.viewmodel.DvirDefect

@Composable
fun DvirDefectsSection(
    defects: List<DvirDefect>,
    onAddDefect: () -> Unit,
    onRemoveDefect: (Int) -> Unit,
    modifier: Modifier = Modifier
) {
    CardContainer(modifier = modifier) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = "Defects Found",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold
                )
                TextButton(onClick = onAddDefect) {
                    Icon(Icons.Filled.Add, contentDescription = null)
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("Add Defect")
                }
            }

            if (defects.isEmpty()) {
                Text(
                    text = "No defects reported. Tap 'Add Defect' if you find any issues.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.padding(vertical = 8.dp)
                )
            } else {
                Spacer(modifier = Modifier.height(8.dp))
                defects.forEachIndexed { index, defect ->
                    DvirDefectItem(
                        defect = defect,
                        onRemove = { onRemoveDefect(index) }
                    )
                    if (index < defects.size - 1) {
                        HorizontalDivider(modifier = Modifier.padding(vertical = 8.dp))
                    }
                }
            }
        }
    }
}

@Composable
fun DvirDefectItem(
    defect: DvirDefect,
    onRemove: () -> Unit,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = modifier.fillMaxWidth(),
        verticalAlignment = Alignment.Top
    ) {
        Icon(
            imageVector = Icons.Filled.Warning,
            contentDescription = null,
            tint = when (defect.severity) {
                DefectSeverity.OUT_OF_SERVICE -> MaterialTheme.colorScheme.error
                DefectSeverity.MAJOR -> MaterialTheme.colorScheme.tertiary
                DefectSeverity.MINOR -> MaterialTheme.colorScheme.secondary
            },
            modifier = Modifier.size(20.dp)
        )
        Spacer(modifier = Modifier.width(8.dp))
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = defect.category.displayName,
                style = MaterialTheme.typography.bodyMedium,
                fontWeight = FontWeight.Bold
            )
            Text(
                text = defect.description,
                style = MaterialTheme.typography.bodySmall
            )
            Text(
                text = defect.severity.displayName,
                style = MaterialTheme.typography.labelSmall,
                color = when (defect.severity) {
                    DefectSeverity.OUT_OF_SERVICE -> MaterialTheme.colorScheme.error
                    DefectSeverity.MAJOR -> MaterialTheme.colorScheme.tertiary
                    DefectSeverity.MINOR -> MaterialTheme.colorScheme.secondary
                }
            )
        }
        IconButton(onClick = onRemove) {
            Icon(Icons.Filled.Close, contentDescription = "Remove")
        }
    }
}
