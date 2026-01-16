package com.jfleets.driver.util

import kotlinx.datetime.DatePeriod
import kotlinx.datetime.TimeZone
import kotlinx.datetime.minus
import kotlinx.datetime.number
import kotlinx.datetime.toLocalDateTime
import kotlin.time.Clock
import kotlin.time.Duration.Companion.days
import kotlin.time.Duration.Companion.hours
import kotlin.time.Duration.Companion.minutes
import kotlin.time.ExperimentalTime
import kotlin.time.Instant

fun Instant.formatShort(): String {
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val month = getMonthAbbreviation(localDateTime.month.number)
    return "$month ${localDateTime.day}, ${localDateTime.year}"
}

/**
 * Formats an instant for display in message lists.
 * Shows relative time for recent messages, date for older ones.
 */
@OptIn(ExperimentalTime::class)
fun Instant.formatMessageTime(): String {
    val now = Clock.System.now()
    val diff = now - this
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val nowLocal = now.toLocalDateTime(TimeZone.currentSystemDefault())

    return when {
        diff < 1.minutes -> "Just now"
        diff < 1.hours -> "${diff.inWholeMinutes}m"
        diff < 24.hours -> "${diff.inWholeHours}h"
        localDateTime.date == nowLocal.date.minus(DatePeriod(days = 1)) -> "Yesterday"
        diff < 7.days -> localDateTime.dayOfWeek.name.take(3).lowercase()
            .replaceFirstChar { it.uppercase() }

        localDateTime.year == nowLocal.year -> "${localDateTime.month.number}/${localDateTime.day}"
        else -> "${localDateTime.month.number}/${localDateTime.day}/${localDateTime.year}"
    }
}

/**
 * Formats a time string for display (e.g., "10:30 AM")
 */
fun Instant.formatTime(): String {
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val hour = localDateTime.hour
    val minute = localDateTime.minute
    val amPm = if (hour < 12) "AM" else "PM"
    val displayHour = when {
        hour == 0 -> 12
        hour > 12 -> hour - 12
        else -> hour
    }
    return "$displayHour:${minute.toString().padStart(2, '0')} $amPm"
}

fun getMonthAbbreviation(month: Int): String {
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
