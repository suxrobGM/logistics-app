package com.logisticsx.driver.ui.components.charts.common

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp

/** One entry in a chart legend. */
data class LegendItem(val label: String, val color: Color)

/** Horizontal legend: a colour swatch and label per data series. */
@Composable
fun ChartLegend(
    items: List<LegendItem>,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically
    ) {
        items.forEach { item ->
            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier.padding(end = 16.dp)
            ) {
                Canvas(modifier = Modifier.size(12.dp)) {
                    drawRoundRect(
                        color = item.color,
                        cornerRadius = CornerRadius(4f, 4f)
                    )
                }
                Spacer(modifier = Modifier.width(4.dp))
                Text(
                    text = item.label,
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

/**
 * Tooltip shown when a chart element is tapped.
 *
 * [values] are name-value pairs, such as "Gross" to "$1,500".
 */
@Composable
fun ChartTooltip(
    label: String,
    values: List<Pair<String, String>>,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier
            .fillMaxWidth()
            .padding(bottom = 8.dp)
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.labelMedium,
            color = MaterialTheme.colorScheme.primary
        )
        values.forEach { (name, value) ->
            Row {
                Text(
                    text = "$name: ",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Text(
                    text = value,
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurface
                )
            }
        }
    }
}

/** Placeholder shown in place of a chart that has no data. */
@Composable
fun EmptyChartPlaceholder(
    modifier: Modifier = Modifier,
    height: Dp = 200.dp,
    message: String = "No data available"
) {
    Box(
        modifier = modifier
            .fillMaxWidth()
            .height(height),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = message,
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}
