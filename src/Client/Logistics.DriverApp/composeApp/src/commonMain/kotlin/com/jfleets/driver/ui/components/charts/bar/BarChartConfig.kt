package com.jfleets.driver.ui.components.charts.bar

/**
 * Configuration options for customizing [BarChart] appearance and behavior.
 *
 * @property showSecondaryBars Whether to display the secondary data series
 * @property animationDuration Duration of the bar growth animation in milliseconds
 * @property cornerRadius Radius for rounded bar corners in pixels
 * @property barSpacing Spacing between bar groups as a fraction of group width (0.0-1.0)
 */
data class BarChartConfig(
    val showSecondaryBars: Boolean = true,
    val animationDuration: Int = 800,
    val cornerRadius: Float = 8f,
    val barSpacing: Float = 0.2f
)
