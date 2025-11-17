package com.jfleets.driver.shared.util

import kotlin.time.Instant
import kotlinx.datetime.TimeZone
import kotlinx.datetime.number
import kotlinx.datetime.toLocalDateTime
import kotlin.math.abs
import kotlin.math.pow
import kotlin.math.round

/**
 * Common extension functions for Kotlin Multiplatform
 * These work on all platforms (Android, iOS, etc.)
 */

// Distance extensions
fun Double.toMiles(): Double = this * 0.000621371

fun Double.formatDistance(): String {
    val miles = this.toMiles()
    return "${miles.formatDecimal(1)} mi"
}

// Currency extensions
fun Double.formatCurrency(): String {
    return "$${this.formatDecimal(2)}"
}

// Date extensions for kotlin.time.Instant
fun Instant.formatShort(): String {
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val month = getMonthAbbreviation(localDateTime.month.number)
    return "$month ${localDateTime.day}, ${localDateTime.year}"
}

fun Instant.formatDateTime(): String {
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val month = getMonthAbbreviation(localDateTime.month.number)
    val hour = localDateTime.hour.toString().padStart(2, '0')
    val minute = localDateTime.minute.toString().padStart(2, '0')
    return "$month ${localDateTime.day}, ${localDateTime.year} $hour:$minute"
}

private fun getMonthAbbreviation(month: Int): String {
    return when (month) {
        1 -> "Jan"
        2 -> "Feb"
        3 -> "Mar"
        4 -> "Apr"
        5 -> "May"
        6 -> "Jun"
        7 -> "Jul"
        8 -> "Aug"
        9 -> "Sep"
        10 -> "Oct"
        11 -> "Nov"
        12 -> "Dec"
        else -> ""
    }
}

// String extensions
fun String.toTitleCase(): String {
    return this.lowercase()
        .split(" ")
        .joinToString(" ") { it.replaceFirstChar { char -> char.uppercase() } }
}

// KMP-compatible decimal formatting helper
private fun Double.formatDecimal(decimalPlaces: Int): String {
    val multiplier = 10.0.pow(decimalPlaces)
    val rounded = round(this * multiplier) / multiplier

    return buildString {
        val intPart = rounded.toInt()
        append(intPart)
        append('.')

        val fracPart = abs((rounded - intPart) * multiplier).toInt()
        append(fracPart.toString().padStart(decimalPlaces, '0'))
    }
}
