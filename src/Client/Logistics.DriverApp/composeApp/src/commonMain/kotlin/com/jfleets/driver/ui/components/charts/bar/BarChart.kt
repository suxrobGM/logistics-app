package com.jfleets.driver.ui.components.charts.bar

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
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
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

/**
 * A bar chart composable that displays financial data with animated bars.
 * Displays two data (Gross and Driver Share) series as grouped bars for each data point.
 * Supports touch interaction to show detailed tooltips for selected bars.
 *
 * @param data List of [ChartData] points to display
 * @param modifier Optional modifier for the chart container
 * @param config Configuration options for chart appearance
 */
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
                        val barWidth = size.width.toFloat() / data.size
                        val tappedIndex = (offset.x / barWidth).toInt()
                        selectedIndex = if (selectedIndex == tappedIndex) null
                            else tappedIndex.coerceIn(data.indices)
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

            val groupWidth = dimensions.chartWidth / data.size
            val barWidth = groupWidth * (1 - config.barSpacing) / 2
            val spacing = groupWidth * config.barSpacing / 2

            data.forEachIndexed { index, item ->
                val groupStartX = dimensions.leftPadding + index * groupWidth + spacing

                // Primary bar (Gross)
                val primaryBarHeight = ((item.gross / maxValue) * dimensions.chartHeight * animationProgress.value).toFloat()
                drawRoundRect(
                    color = if (selectedIndex == index) primaryColor.copy(alpha = 0.8f) else primaryColor,
                    topLeft = Offset(groupStartX, dimensions.chartHeight - primaryBarHeight),
                    size = Size(barWidth, primaryBarHeight),
                    cornerRadius = CornerRadius(config.cornerRadius, config.cornerRadius)
                )

                // Secondary bar (Driver Share)
                if (config.showSecondaryBars) {
                    val secondaryBarHeight = ((item.driverShare / maxValue) * dimensions.chartHeight * animationProgress.value).toFloat()
                    drawRoundRect(
                        color = if (selectedIndex == index) secondaryColor.copy(alpha = 0.8f) else secondaryColor,
                        topLeft = Offset(groupStartX + barWidth + 4f, dimensions.chartHeight - secondaryBarHeight),
                        size = Size(barWidth, secondaryBarHeight),
                        cornerRadius = CornerRadius(config.cornerRadius, config.cornerRadius)
                    )
                }

                // X-axis label
                val labelText = formatXAxisLabel(item.label, XAxisLabelStyle.MONTH_DAY)
                drawXAxisLabel(
                    label = labelText,
                    x = groupStartX + groupWidth / 2 - spacing,
                    chartHeight = dimensions.chartHeight,
                    textMeasurer = textMeasurer,
                    textColor = onSurfaceColor
                )
            }
        }
    }
}
