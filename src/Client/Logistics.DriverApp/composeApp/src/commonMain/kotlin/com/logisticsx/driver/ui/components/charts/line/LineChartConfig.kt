package com.logisticsx.driver.ui.components.charts.line

/**
 * Appearance options for [LineChart].
 *
 * The secondary series is Driver Share. [animationDuration] is in milliseconds; [lineWidth] and
 * [pointRadius] are in pixels.
 */
data class LineChartConfig(
    val showSecondaryLine: Boolean = true,
    val showFill: Boolean = true,
    val showPoints: Boolean = true,
    val animationDuration: Int = 1000,
    val lineWidth: Float = 3f,
    val pointRadius: Float = 6f
)
