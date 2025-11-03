package com.jfleets.driver.shared.util

import kotlinx.datetime.Instant
import kotlinx.datetime.TimeZone
import kotlinx.datetime.toLocalDateTime

/**
 * Common extension functions for Kotlin Multiplatform
 * These work on all platforms (Android, iOS, etc.)
 */

// Distance extensions
fun Double.toMiles(): Double = this * 0.000621371

fun Double.formatDistance(): String {
    val miles = this.toMiles()
    return String.format("%.1f mi", miles)
}

// Currency extensions
fun Double.formatCurrency(): String {
    return "$%.2f".format(this)
}

// Date extensions for kotlinx.datetime.Instant
fun Instant.formatShort(): String {
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val month = getMonthAbbreviation(localDateTime.monthNumber)
    return "$month ${localDateTime.dayOfMonth}, ${localDateTime.year}"
}

fun Instant.formatDateTime(): String {
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val month = getMonthAbbreviation(localDateTime.monthNumber)
    val hour = localDateTime.hour.toString().padStart(2, '0')
    val minute = localDateTime.minute.toString().padStart(2, '0')
    return "$month ${localDateTime.dayOfMonth}, ${localDateTime.year} $hour:$minute"
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
