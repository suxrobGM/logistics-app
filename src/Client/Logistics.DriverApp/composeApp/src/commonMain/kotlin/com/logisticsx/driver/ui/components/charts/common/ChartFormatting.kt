package com.logisticsx.driver.ui.components.charts.common

import com.logisticsx.driver.util.getMonthAbbreviation

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
 * Formats a date string for a chart X-axis.
 *
 * A full date such as "2024-01-15" follows [style]. A month such as "2024-01" always becomes a
 * month abbreviation. Anything else is truncated to its first six characters.
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
        else -> label.take(6)
    }
}
