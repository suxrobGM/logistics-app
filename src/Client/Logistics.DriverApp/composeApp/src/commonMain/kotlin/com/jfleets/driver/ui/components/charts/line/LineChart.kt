package com.jfleets.driver.ui.components.charts.line

import androidx.compose.animation.core.Animatable
import androidx.compose.animation.core.tween
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
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
import androidx.compose.ui.text.rememberTextMeasurer
import androidx.compose.ui.unit.dp
import com.jfleets.driver.model.ChartData
import com.jfleets.driver.ui.components.charts.common.ChartDimensions
import com.jfleets.driver.ui.components.charts.common.ChartLegend
import com.jfleets.driver.ui.components.charts.common.ChartTooltip
import com.jfleets.driver.ui.components.charts.common.EmptyChartPlaceholder
import com.jfleets.driver.ui.components.charts.common.LegendItem
import com.jfleets.driver.ui.components.charts.common.XAxisLabelStyle
import com.jfleets.driver.ui.components.charts.common.drawGridLines
import com.jfleets.driver.ui.components.charts.common.drawXAxisLabel
import com.jfleets.driver.ui.components.charts.common.drawYAxisLabels
import com.jfleets.driver.ui.components.charts.common.formatXAxisLabel
import com.jfleets.driver.util.formatCurrency
import kotlin.math.abs

/**
 * A line chart composable that displays financial data with animated lines.
 * Displays two data series (Gross and Driver Share) as connected lines with optional
 * gradient fill and data point indicators. Supports touch interaction for tooltips.
 *
 * @param data List of [ChartData] points to display
 * @param modifier Optional modifier for the chart container
 * @param config Configuration options for chart appearance
 */
@Composable
fun LineChart(
    data: List<ChartData>,
    modifier: Modifier = Modifier,
    config: LineChartConfig = LineChartConfig()
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
        ChartLegend(
            items = listOf(
                LegendItem("Gross", primaryColor),
                LegendItem("Driver Share", secondaryColor)
            )
        )

        Spacer(modifier = Modifier.height(8.dp))

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

                        selectedIndex = if (minDistance < pointSpacing / 2) {
                            if (selectedIndex == nearestIndex) null else nearestIndex
                        } else null
                    }
                }
        ) {
            val dimensions = ChartDimensions(size.width, size.height)

            drawYAxisLabels(
                maxValue = maxValue,
                dimensions = dimensions,
                textMeasurer = textMeasurer,
                textColor = onSurfaceColor
            )

            drawGridLines(
                dimensions = dimensions,
                lineColor = surfaceVariantColor
            )

            if (data.size == 1) {
                drawSinglePoint(
                    data = data[0],
                    dimensions = dimensions,
                    maxValue = maxValue,
                    primaryColor = primaryColor,
                    secondaryColor = secondaryColor,
                    config = config
                )
            } else {
                val pointSpacing = dimensions.chartWidth / (data.size - 1)
                val visibleDataCount = (data.size * animationProgress.value).toInt().coerceAtLeast(1)

                drawDataLine(
                    data = data.take(visibleDataCount),
                    getValue = { it.gross },
                    maxValue = maxValue,
                    dimensions = dimensions,
                    pointSpacing = pointSpacing,
                    lineColor = primaryColor,
                    config = config,
                    selectedIndex = selectedIndex
                )

                if (config.showSecondaryLine) {
                    drawDataLine(
                        data = data.take(visibleDataCount),
                        getValue = { it.driverShare },
                        maxValue = maxValue,
                        dimensions = dimensions,
                        pointSpacing = pointSpacing,
                        lineColor = secondaryColor,
                        config = config,
                        selectedIndex = selectedIndex
                    )
                }
            }

            // X-axis labels
            val labelStep = when {
                data.size <= 7 -> 1
                data.size <= 14 -> 2
                data.size <= 31 -> 5
                else -> data.size / 6
            }

            data.forEachIndexed { index, item ->
                if (index % labelStep == 0 || index == data.size - 1) {
                    val x = if (data.size == 1) {
                        dimensions.leftPadding + dimensions.chartWidth / 2
                    } else {
                        dimensions.leftPadding + index * (dimensions.chartWidth / (data.size - 1))
                    }

                    drawXAxisLabel(
                        label = formatXAxisLabel(item.label, XAxisLabelStyle.DAY_ONLY),
                        x = x,
                        chartHeight = dimensions.chartHeight,
                        textMeasurer = textMeasurer,
                        textColor = onSurfaceColor
                    )
                }
            }
        }
    }
}

/**
 * Draws a single data point when only one data entry exists.
 * Centers the point horizontally and draws both primary and secondary indicators.
 */
private fun DrawScope.drawSinglePoint(
    data: ChartData,
    dimensions: ChartDimensions,
    maxValue: Double,
    primaryColor: Color,
    secondaryColor: Color,
    config: LineChartConfig
) {
    val x = dimensions.leftPadding + dimensions.chartWidth / 2
    val y = dimensions.chartHeight - ((data.gross / maxValue) * dimensions.chartHeight).toFloat()

    drawCircle(color = primaryColor, radius = config.pointRadius, center = Offset(x, y))

    if (config.showSecondaryLine) {
        val y2 = dimensions.chartHeight - ((data.driverShare / maxValue) * dimensions.chartHeight).toFloat()
        drawCircle(color = secondaryColor, radius = config.pointRadius, center = Offset(x, y2))
    }
}

/**
 * Draws a complete data line with optional fill gradient and point indicators.
 *
 * @param data List of chart data points
 * @param getValue Function to extract the value from each data point
 * @param maxValue Maximum value for Y-axis scaling
 * @param dimensions Chart dimensions for positioning
 * @param pointSpacing Horizontal spacing between points
 * @param lineColor Color for the line and points
 * @param config Line chart configuration
 * @param selectedIndex Currently selected point index (if any)
 */
private fun DrawScope.drawDataLine(
    data: List<ChartData>,
    getValue: (ChartData) -> Double,
    maxValue: Double,
    dimensions: ChartDimensions,
    pointSpacing: Float,
    lineColor: Color,
    config: LineChartConfig,
    selectedIndex: Int?
) {
    if (data.isEmpty()) return

    val path = Path()
    val fillPath = Path()

    data.forEachIndexed { index, item ->
        val x = dimensions.leftPadding + index * pointSpacing
        val y = dimensions.chartHeight - ((getValue(item) / maxValue) * dimensions.chartHeight).toFloat()

        if (index == 0) {
            path.moveTo(x, y)
            fillPath.moveTo(x, dimensions.chartHeight)
            fillPath.lineTo(x, y)
        } else {
            path.lineTo(x, y)
            fillPath.lineTo(x, y)
        }
    }

    // Fill gradient
    if (config.showFill && data.size > 1) {
        val lastX = dimensions.leftPadding + (data.size - 1) * pointSpacing
        fillPath.lineTo(lastX, dimensions.chartHeight)
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

    // Line
    drawPath(
        path = path,
        color = lineColor,
        style = Stroke(width = config.lineWidth, cap = StrokeCap.Round, join = StrokeJoin.Round)
    )

    // Points
    if (config.showPoints) {
        data.forEachIndexed { index, item ->
            val x = dimensions.leftPadding + index * pointSpacing
            val y = dimensions.chartHeight - ((getValue(item) / maxValue) * dimensions.chartHeight).toFloat()

            drawCircle(
                color = lineColor,
                radius = if (selectedIndex == index) config.pointRadius * 1.5f else config.pointRadius,
                center = Offset(x, y)
            )
            drawCircle(
                color = Color.White,
                radius = if (selectedIndex == index) config.pointRadius else config.pointRadius * 0.5f,
                center = Offset(x, y)
            )
        }
    }
}
