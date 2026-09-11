package com.logisticsx.driver.ui.components.charts.bar

/**
 * Appearance options for [BarChart].
 *
 * [animationDuration] is in milliseconds and [cornerRadius] in pixels. [barSpacing] is a fraction
 * of the group width, from 0.0 to 1.0.
 */
data class BarChartConfig(
    val showSecondaryBars: Boolean = true,
    val animationDuration: Int = 800,
    val cornerRadius: Float = 8f,
    val barSpacing: Float = 0.2f
)
