package com.jfleets.driver.ui.components.charts.line

/**
 * Configuration options for customizing [LineChart] appearance and behavior.
 *
 * @property showSecondaryLine Whether to display the secondary data series (Driver Share)
 * @property showFill Whether to show gradient fill under the line
 * @property showPoints Whether to show data point indicators on the line
 * @property animationDuration Duration of the line drawing animation in milliseconds
 * @property lineWidth Stroke width of the line in pixels
 * @property pointRadius Radius of data point circles in pixels
 */
data class LineChartConfig(
    val showSecondaryLine: Boolean = true,
    val showFill: Boolean = true,
    val showPoints: Boolean = true,
    val animationDuration: Int = 1000,
    val lineWidth: Float = 3f,
    val pointRadius: Float = 6f
)
