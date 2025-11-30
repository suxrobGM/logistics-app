package com.jfleets.driver.presentation.ui.components.charts

import androidx.compose.animation.core.Animatable
import androidx.compose.animation.core.tween
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectTapGestures
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
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.DrawScope
import androidx.compose.ui.graphics.nativeCanvas
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.text.TextMeasurer
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.drawText
import androidx.compose.ui.text.rememberTextMeasurer
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.jfleets.driver.model.ChartData
import com.jfleets.driver.util.formatCurrency

data class BarChartConfig(
    val barColor: Color = Color(0xFF4CAF50),
    val secondaryBarColor: Color = Color(0xFF2196F3),
    val showSecondaryBars: Boolean = true,
    val animationDuration: Int = 800,
    val cornerRadius: Float = 8f,
    val barSpacing: Float = 0.2f
)

@Composable
fun BarChart(
    data: List<ChartData>,
    modifier: Modifier = Modifier,
    config: BarChartConfig = BarChartConfig()
) {
    if (data.isEmpty()) {
        EmptyChartPlaceholder(modifier)
        return
    }

    val primaryColor = MaterialTheme.colorScheme.primary
    val secondaryColor = MaterialTheme.colorScheme.secondary
    val onSurfaceColor = MaterialTheme.colorScheme.onSurface
    val surfaceVariantColor = MaterialTheme.colorScheme.surfaceVariant

    val textMeasurer = rememberTextMeasurer()
    val animationProgress = remember { Animatable(0f) }
    var selectedIndex by remember { mutableStateOf<Int?>(null) }

    LaunchedEffect(data) {
        animationProgress.snapTo(0f)
        animationProgress.animateTo(1f, animationSpec = tween(config.animationDuration))
    }

    val maxValue = remember(data) {
        data.maxOfOrNull { maxOf(it.gross, it.driverShare) } ?: 1.0
    }

    Column(modifier = modifier) {
        // Legend
        ChartLegend(
            items = listOf(
                LegendItem("Gross", primaryColor),
                LegendItem("Driver Share", secondaryColor)
            )
        )

        Spacer(modifier = Modifier.height(8.dp))

        // Selected bar tooltip
        selectedIndex?.let { index ->
            if (index in data.indices) {
                val item = data[index]
                ChartTooltip(
                    label = item.label,
                    values = listOf(
                        "Gross" to item.gross.formatCurrency(),
                        "Driver Share" to item.driverShare.formatCurrency()
                    )
                )
            }
        }

        Canvas(
            modifier = Modifier
                .fillMaxWidth()
                .height(200.dp)
                .pointerInput(data) {
                    detectTapGestures { offset ->
                        val barWidth = size.width.toFloat() / data.size
                        val tappedIndex = (offset.x / barWidth).toInt()
                        selectedIndex = if (selectedIndex == tappedIndex) null else tappedIndex.coerceIn(data.indices)
                    }
                }
        ) {
            val canvasWidth = size.width
            val canvasHeight = size.height
            val bottomPadding = 40f
            val leftPadding = 50f
            val chartHeight = canvasHeight - bottomPadding
            val chartWidth = canvasWidth - leftPadding

            // Draw Y-axis labels
            drawYAxisLabels(
                maxValue = maxValue,
                chartHeight = chartHeight,
                leftPadding = leftPadding,
                textMeasurer = textMeasurer,
                textColor = onSurfaceColor
            )

            // Draw horizontal grid lines
            drawGridLines(
                chartHeight = chartHeight,
                chartWidth = chartWidth,
                leftPadding = leftPadding,
                lineColor = surfaceVariantColor
            )

            // Draw bars
            val groupWidth = chartWidth / data.size
            val barWidth = groupWidth * (1 - config.barSpacing) / 2
            val spacing = groupWidth * config.barSpacing / 2

            data.forEachIndexed { index, item ->
                val groupStartX = leftPadding + index * groupWidth + spacing

                // Primary bar (Gross)
                val primaryBarHeight = ((item.gross / maxValue) * chartHeight * animationProgress.value).toFloat()
                drawRoundRect(
                    color = if (selectedIndex == index) primaryColor.copy(alpha = 0.8f) else primaryColor,
                    topLeft = Offset(groupStartX, chartHeight - primaryBarHeight),
                    size = Size(barWidth, primaryBarHeight),
                    cornerRadius = CornerRadius(config.cornerRadius, config.cornerRadius)
                )

                // Secondary bar (Driver Share)
                if (config.showSecondaryBars) {
                    val secondaryBarHeight = ((item.driverShare / maxValue) * chartHeight * animationProgress.value).toFloat()
                    drawRoundRect(
                        color = if (selectedIndex == index) secondaryColor.copy(alpha = 0.8f) else secondaryColor,
                        topLeft = Offset(groupStartX + barWidth + 4f, chartHeight - secondaryBarHeight),
                        size = Size(barWidth, secondaryBarHeight),
                        cornerRadius = CornerRadius(config.cornerRadius, config.cornerRadius)
                    )
                }

                // Draw X-axis label
                val labelText = formatXAxisLabel(item.label)
                val labelWidth = textMeasurer.measure(labelText).size.width
                drawText(
                    textMeasurer = textMeasurer,
                    text = labelText,
                    topLeft = Offset(
                        x = groupStartX + groupWidth / 2 - labelWidth / 2 - spacing,
                        y = chartHeight + 8f
                    ),
                    style = TextStyle(
                        color = onSurfaceColor,
                        fontSize = 10.sp
                    )
                )
            }
        }
    }
}

private fun DrawScope.drawYAxisLabels(
    maxValue: Double,
    chartHeight: Float,
    leftPadding: Float,
    textMeasurer: TextMeasurer,
    textColor: Color
) {
    val steps = 4
    for (i in 0..steps) {
        val value = (maxValue * i / steps)
        val y = chartHeight - (chartHeight * i / steps)
        val labelText = formatYAxisValue(value)

        drawText(
            textMeasurer = textMeasurer,
            text = labelText,
            topLeft = Offset(0f, y - 8f),
            style = TextStyle(
                color = textColor,
                fontSize = 10.sp
            )
        )
    }
}

private fun DrawScope.drawGridLines(
    chartHeight: Float,
    chartWidth: Float,
    leftPadding: Float,
    lineColor: Color
) {
    val steps = 4
    for (i in 0..steps) {
        val y = chartHeight - (chartHeight * i / steps)
        drawLine(
            color = lineColor.copy(alpha = 0.5f),
            start = Offset(leftPadding, y),
            end = Offset(leftPadding + chartWidth, y),
            strokeWidth = 1f
        )
    }
}

private fun formatYAxisValue(value: Double): String {
    return when {
        value >= 1000 -> "${(value / 1000).toInt()}k"
        value >= 1 -> value.toInt().toString()
        else -> ""
    }
}

private fun formatXAxisLabel(label: String): String {
    // Handle date formats like "2024-01-15" -> "Jan 15" or "2024-01" -> "Jan"
    return when {
        label.matches(Regex("\\d{4}-\\d{2}-\\d{2}")) -> {
            val parts = label.split("-")
            val month = getMonthAbbr(parts[1].toIntOrNull() ?: 1)
            val day = parts[2].toIntOrNull() ?: 1
            "$month $day"
        }
        label.matches(Regex("\\d{4}-\\d{2}")) -> {
            val parts = label.split("-")
            getMonthAbbr(parts[1].toIntOrNull() ?: 1)
        }
        else -> label.take(6)
    }
}

private fun getMonthAbbr(month: Int): String {
    return listOf("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec")
        .getOrElse(month - 1) { "???" }
}

@Composable
private fun EmptyChartPlaceholder(modifier: Modifier = Modifier) {
    Box(
        modifier = modifier
            .fillMaxWidth()
            .height(200.dp),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = "No data available",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

data class LegendItem(val label: String, val color: Color)

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
