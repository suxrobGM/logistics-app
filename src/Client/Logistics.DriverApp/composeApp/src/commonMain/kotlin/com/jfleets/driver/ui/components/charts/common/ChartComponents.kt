package com.jfleets.driver.ui.components.charts.common

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

/**
 * Represents a single item in a chart legend.
 *
 * @property label The text label for the legend item
 * @property color The color associated with this data series
 */
data class LegendItem(val label: String, val color: Color)

/**
 * Displays a horizontal legend for chart data series.
 * Shows colored indicators with labels for each data series in the chart.
 *
 * @param items List of legend items to display
 * @param modifier Optional modifier for the legend container
 */
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
 * Displays a tooltip with detailed information for a selected chart data point.
 *
 * Shows the label and associated values when a user taps on a chart element.
 *
 * @param label The primary label for the tooltip (e.g., date)
 * @param values List of name-value pairs to display (e.g., "Gross" to "$1,500")
 * @param modifier Optional modifier for the tooltip container
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

/**
 * Displays a placeholder when chart has no data to show.
 *
 * @param modifier Optional modifier for the placeholder container
 * @param height Height of the placeholder area
 * @param message Message to display in the placeholder
 */
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
