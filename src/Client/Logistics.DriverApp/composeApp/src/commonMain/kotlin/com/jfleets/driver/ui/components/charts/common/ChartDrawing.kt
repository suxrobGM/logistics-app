package com.jfleets.driver.ui.components.charts.common

import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.DrawScope
import androidx.compose.ui.text.TextMeasurer
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.drawText
import androidx.compose.ui.unit.sp

/**
 * Holds calculated dimensions for chart drawing.
 * Provides computed properties for the actual drawable chart area
 * after accounting for padding used by axis labels.
 *
 * @property canvasWidth Total width of the canvas
 * @property canvasHeight Total height of the canvas
 * @property leftPadding Space reserved on the left for Y-axis labels
 * @property bottomPadding Space reserved at the bottom for X-axis labels
 */
data class ChartDimensions(
    val canvasWidth: Float,
    val canvasHeight: Float,
    val leftPadding: Float = 50f,
    val bottomPadding: Float = 40f
) {
    /** Available width for drawing chart content */
    val chartWidth: Float get() = canvasWidth - leftPadding

    /** Available height for drawing chart content */
    val chartHeight: Float get() = canvasHeight - bottomPadding
}

/**
 * Draws Y-axis labels on the left side of the chart.
 * Labels are evenly distributed from 0 to [maxValue] across [steps] intervals.
 * Values are formatted with "k" suffix for thousands (e.g., "5k" for 5000).
 *
 * @param maxValue The maximum value represented on the Y-axis
 * @param dimensions Chart dimensions for positioning
 * @param textMeasurer Text measurer for label sizing
 * @param textColor Color for the label text
 * @param steps Number of intervals to display (default: 4)
 */
fun DrawScope.drawYAxisLabels(
    maxValue: Double,
    dimensions: ChartDimensions,
    textMeasurer: TextMeasurer,
    textColor: Color,
    steps: Int = 4
) {
    for (i in 0..steps) {
        val value = (maxValue * i / steps)
        val y = dimensions.chartHeight - (dimensions.chartHeight * i / steps)
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

/**
 * Draws horizontal grid lines across the chart area.
 * Grid lines help users read values by providing visual reference points.
 * Lines are drawn with reduced opacity for subtle appearance.
 *
 * @param dimensions Chart dimensions for positioning
 * @param lineColor Color for the grid lines (will be drawn at 50% opacity)
 * @param steps Number of grid lines to draw (default: 4)
 */
fun DrawScope.drawGridLines(
    dimensions: ChartDimensions,
    lineColor: Color,
    steps: Int = 4
) {
    for (i in 0..steps) {
        val y = dimensions.chartHeight - (dimensions.chartHeight * i / steps)
        drawLine(
            color = lineColor.copy(alpha = 0.5f),
            start = Offset(dimensions.leftPadding, y),
            end = Offset(dimensions.leftPadding + dimensions.chartWidth, y),
            strokeWidth = 1f
        )
    }
}

/**
 * Draws a centered X-axis label at the specified position.
 * The label is horizontally centered on the given X coordinate
 * and positioned below the chart area.
 *
 * @param label Text to display
 * @param x X coordinate for the center of the label
 * @param chartHeight Height of the chart area (label draws below this)
 * @param textMeasurer Text measurer for label sizing
 * @param textColor Color for the label text
 */
fun DrawScope.drawXAxisLabel(
    label: String,
    x: Float,
    chartHeight: Float,
    textMeasurer: TextMeasurer,
    textColor: Color
) {
    val labelWidth = textMeasurer.measure(label).size.width
    drawText(
        textMeasurer = textMeasurer,
        text = label,
        topLeft = Offset(
            x = x - labelWidth / 2,
            y = chartHeight + 8f
        ),
        style = TextStyle(
            color = textColor,
            fontSize = 10.sp
        )
    )
}

/**
 * Formats a numeric value for Y-axis display.
 *
 * @param value The value to format
 * @return Formatted string: "5k" for 5000, "500" for 500, "" for values < 1
 */
private fun formatYAxisValue(value: Double): String {
    return when {
        value >= 1000 -> "${(value / 1000).toInt()}k"
        value >= 1 -> value.toInt().toString()
        else -> ""
    }
}
