package com.logisticsx.driver.ui.components.charts.common

import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.DrawScope
import androidx.compose.ui.text.TextMeasurer
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.drawText
import androidx.compose.ui.unit.sp

/** Canvas size plus the padding reserved for axis labels, and the drawable area left over. */
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
 * Draws Y-axis labels, evenly spaced from 0 to [maxValue] across [steps] intervals.
 *
 * Thousands are abbreviated, so 5000 renders as "5k".
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

/** Draws [steps] horizontal grid lines across the chart, at 50% of [lineColor]. */
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

/** Draws an X-axis label centred on [x], below [chartHeight]. */
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

/** Formats a Y-axis value: 5000 becomes "5k", 500 stays "500", anything below 1 becomes "". */
private fun formatYAxisValue(value: Double): String {
    return when {
        value >= 1000 -> "${(value / 1000).toInt()}k"
        value >= 1 -> value.toInt().toString()
        else -> ""
    }
}
