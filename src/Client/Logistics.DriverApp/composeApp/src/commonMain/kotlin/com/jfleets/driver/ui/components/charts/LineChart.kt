package com.jfleets.driver.ui.components.charts

import androidx.compose.animation.core.Animatable
import androidx.compose.animation.core.tween
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
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
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.drawscope.DrawScope
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.drawText
import androidx.compose.ui.text.rememberTextMeasurer
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.jfleets.driver.model.ChartData
import com.jfleets.driver.util.formatCurrency
import kotlin.math.abs

data class LineChartConfig(
    val lineColor: Color = Color(0xFF4CAF50),
    val secondaryLineColor: Color = Color(0xFF2196F3),
    val showSecondaryLine: Boolean = true,
    val showFill: Boolean = true,
    val showPoints: Boolean = true,
    val animationDuration: Int = 1000,
    val lineWidth: Float = 3f,
    val pointRadius: Float = 6f
)

@Composable
fun LineChart(
    data: List<ChartData>,
    modifier: Modifier = Modifier,
    config: LineChartConfig = LineChartConfig()
) {
    if (data.isEmpty()) {
        EmptyLineChartPlaceholder(modifier)
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

        // Selected point tooltip
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
                        val leftPadding = 50f
                        val chartWidth = size.width - leftPadding
                        val pointSpacing = chartWidth / (data.size - 1).coerceAtLeast(1)

                        // Find nearest point
                        var nearestIndex = 0
                        var minDistance = Float.MAX_VALUE
                        data.forEachIndexed { index, _ ->
                            val pointX = leftPadding + index * pointSpacing
                            val distance = abs(offset.x - pointX)
                            if (distance < minDistance) {
                                minDistance = distance
                                nearestIndex = index
                            }
                        }

                        if (minDistance < pointSpacing / 2) {
                            selectedIndex =
                                if (selectedIndex == nearestIndex) null else nearestIndex
                        } else {
                            selectedIndex = null
                        }
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
            drawLineChartYAxisLabels(
                maxValue = maxValue,
                chartHeight = chartHeight,
                textMeasurer = textMeasurer,
                textColor = onSurfaceColor
            )

            // Draw horizontal grid lines
            drawLineChartGridLines(
                chartHeight = chartHeight,
                chartWidth = chartWidth,
                leftPadding = leftPadding,
                lineColor = surfaceVariantColor
            )

            if (data.size == 1) {
                // Single data point - draw as a point only
                val x = leftPadding + chartWidth / 2
                val y = chartHeight - ((data[0].gross / maxValue) * chartHeight).toFloat()

                drawCircle(
                    color = primaryColor,
                    radius = config.pointRadius,
                    center = Offset(x, y)
                )

                if (config.showSecondaryLine) {
                    val y2 =
                        chartHeight - ((data[0].driverShare / maxValue) * chartHeight).toFloat()
                    drawCircle(
                        color = secondaryColor,
                        radius = config.pointRadius,
                        center = Offset(x, y2)
                    )
                }
            } else {
                val pointSpacing = chartWidth / (data.size - 1)

                // Calculate visible data based on animation progress
                val visibleDataCount =
                    (data.size * animationProgress.value).toInt().coerceAtLeast(1)

                // Draw primary line (Gross)
                drawLine(
                    data = data.take(visibleDataCount),
                    getValue = { it.gross },
                    maxValue = maxValue,
                    chartHeight = chartHeight,
                    leftPadding = leftPadding,
                    pointSpacing = pointSpacing,
                    lineColor = primaryColor,
                    config = config,
                    selectedIndex = selectedIndex
                )

                // Draw secondary line (Driver Share)
                if (config.showSecondaryLine) {
                    drawLine(
                        data = data.take(visibleDataCount),
                        getValue = { it.driverShare },
                        maxValue = maxValue,
                        chartHeight = chartHeight,
                        leftPadding = leftPadding,
                        pointSpacing = pointSpacing,
                        lineColor = secondaryColor,
                        config = config,
                        selectedIndex = selectedIndex
                    )
                }
            }

            // Draw X-axis labels
            val labelStep = when {
                data.size <= 7 -> 1
                data.size <= 14 -> 2
                data.size <= 31 -> 5
                else -> data.size / 6
            }

            data.forEachIndexed { index, item ->
                if (index % labelStep == 0 || index == data.size - 1) {
                    val x = if (data.size == 1) {
                        leftPadding + chartWidth / 2
                    } else {
                        leftPadding + index * (chartWidth / (data.size - 1))
                    }

                    val labelText = formatLineChartXAxisLabel(item.label)
                    val labelWidth = textMeasurer.measure(labelText).size.width

                    drawText(
                        textMeasurer = textMeasurer,
                        text = labelText,
                        topLeft = Offset(
                            x = x - labelWidth / 2,
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
}

private fun DrawScope.drawLine(
    data: List<ChartData>,
    getValue: (ChartData) -> Double,
    maxValue: Double,
    chartHeight: Float,
    leftPadding: Float,
    pointSpacing: Float,
    lineColor: Color,
    config: LineChartConfig,
    selectedIndex: Int?
) {
    if (data.isEmpty()) return

    val path = Path()
    val fillPath = Path()

    data.forEachIndexed { index, item ->
        val x = leftPadding + index * pointSpacing
        val y = chartHeight - ((getValue(item) / maxValue) * chartHeight).toFloat()

        if (index == 0) {
            path.moveTo(x, y)
            fillPath.moveTo(x, chartHeight)
            fillPath.lineTo(x, y)
        } else {
            path.lineTo(x, y)
            fillPath.lineTo(x, y)
        }
    }

    // Draw fill gradient
    if (config.showFill && data.size > 1) {
        val lastX = leftPadding + (data.size - 1) * pointSpacing
        fillPath.lineTo(lastX, chartHeight)
        fillPath.close()

        drawPath(
            path = fillPath,
            brush = Brush.verticalGradient(
                colors = listOf(
                    lineColor.copy(alpha = 0.3f),
                    lineColor.copy(alpha = 0.05f)
                )
            )
        )
    }

    // Draw line
    drawPath(
        path = path,
        color = lineColor,
        style = Stroke(
            width = config.lineWidth,
            cap = StrokeCap.Round,
            join = StrokeJoin.Round
        )
    )

    // Draw points
    if (config.showPoints) {
        data.forEachIndexed { index, item ->
            val x = leftPadding + index * pointSpacing
            val y = chartHeight - ((getValue(item) / maxValue) * chartHeight).toFloat()

            // Outer circle
            drawCircle(
                color = lineColor,
                radius = if (selectedIndex == index) config.pointRadius * 1.5f else config.pointRadius,
                center = Offset(x, y)
            )

            // Inner circle
            drawCircle(
                color = Color.White,
                radius = if (selectedIndex == index) config.pointRadius else config.pointRadius * 0.5f,
                center = Offset(x, y)
            )
        }
    }
}

private fun DrawScope.drawLineChartYAxisLabels(
    maxValue: Double,
    chartHeight: Float,
    textMeasurer: androidx.compose.ui.text.TextMeasurer,
    textColor: Color
) {
    val steps = 4
    for (i in 0..steps) {
        val value = (maxValue * i / steps)
        val y = chartHeight - (chartHeight * i / steps)
        val labelText = formatLineChartYAxisValue(value)

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

private fun DrawScope.drawLineChartGridLines(
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

private fun formatLineChartYAxisValue(value: Double): String {
    return when {
        value >= 1000 -> "${(value / 1000).toInt()}k"
        value >= 1 -> value.toInt().toString()
        else -> ""
    }
}

private fun formatLineChartXAxisLabel(label: String): String {
    return when {
        label.matches(Regex("\\d{4}-\\d{2}-\\d{2}")) -> {
            val parts = label.split("-")
            getLineChartMonthAbbr(parts[1].toIntOrNull() ?: 1)
            val day = parts[2].toIntOrNull() ?: 1
            "$day"
        }

        label.matches(Regex("\\d{4}-\\d{2}")) -> {
            val parts = label.split("-")
            getLineChartMonthAbbr(parts[1].toIntOrNull() ?: 1)
        }

        else -> label.take(4)
    }
}

private fun getLineChartMonthAbbr(month: Int): String {
    return listOf(
        "Jan",
        "Feb",
        "Mar",
        "Apr",
        "May",
        "Jun",
        "Jul",
        "Aug",
        "Sep",
        "Oct",
        "Nov",
        "Dec"
    )
        .getOrElse(month - 1) { "???" }
}

@Composable
private fun EmptyLineChartPlaceholder(modifier: Modifier = Modifier) {
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
