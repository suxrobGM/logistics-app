package com.jfleets.driver.ui.components.charts.common

import com.jfleets.driver.util.getMonthAbbreviation

/**
 * Defines the display style for X-axis labels in charts.
 */
enum class XAxisLabelStyle {
    /** Display as "Jan 15" format */
    MONTH_DAY,
    /** Display day number only, e.g., "15" */
    DAY_ONLY,
    /** Display month abbreviation only, e.g., "Jan" */
    MONTH_ONLY
}

/**
 * Formats a date string for display on the X-axis of a chart.
 *
 * Supports the following input formats:
 * - Full date: "2024-01-15" -> formatted based on [style]
 * - Month only: "2024-01" -> always returns month abbreviation
 * - Other: returns first 6 characters
 *
 * @param label The date string to format
 * @param style The desired output format style
 * @return Formatted label string for chart display
 */
fun formatXAxisLabel(label: String, style: XAxisLabelStyle = XAxisLabelStyle.MONTH_DAY): String {
    return when {
        // Full date: "2024-01-15"
        label.matches(Regex("\\d{4}-\\d{2}-\\d{2}")) -> {
            val parts = label.split("-")
            val month = getMonthAbbreviation(parts[1].toIntOrNull() ?: 1)
            val day = parts[2].toIntOrNull() ?: 1
            when (style) {
                XAxisLabelStyle.MONTH_DAY -> "$month $day"
                XAxisLabelStyle.DAY_ONLY -> "$day"
                XAxisLabelStyle.MONTH_ONLY -> month
            }
        }
        // Month only: "2024-01"
        label.matches(Regex("\\d{4}-\\d{2}")) -> {
            val parts = label.split("-")
            getMonthAbbreviation(parts[1].toIntOrNull() ?: 1)
        }
        // Fallback
        else -> label.take(6)
    }
}
