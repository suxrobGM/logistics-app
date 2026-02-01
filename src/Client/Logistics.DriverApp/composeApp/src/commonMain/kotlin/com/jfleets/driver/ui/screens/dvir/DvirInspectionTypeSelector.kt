package com.jfleets.driver.ui.screens.dvir

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material3.FilterChip
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.DvirType
import com.jfleets.driver.ui.components.SectionCard
import com.jfleets.driver.util.displayName

@OptIn(ExperimentalLayoutApi::class)
@Composable
fun DvirInspectionTypeSelector(
    selectedType: DvirType,
    onTypeSelected: (DvirType) -> Unit,
    modifier: Modifier = Modifier
) {
    SectionCard(
        title = "Inspection Type",
        modifier = modifier
    ) {
        FlowRow(
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            DvirType.entries.forEach { type ->
                FilterChip(
                    selected = selectedType == type,
                    onClick = { onTypeSelected(type) },
                    label = { Text(type.displayName) },
                    leadingIcon = if (selectedType == type) {
                        {
                            Icon(
                                Icons.Filled.Check,
                                contentDescription = null,
                                Modifier.size(18.dp)
                            )
                        }
                    } else null
                )
            }
        }
    }
}
