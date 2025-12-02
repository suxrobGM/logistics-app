package com.jfleets.driver.util

import kotlinx.datetime.TimeZone
import kotlinx.datetime.number
import kotlinx.datetime.toLocalDateTime
import kotlin.time.Instant

fun Instant.formatShort(): String {
    val localDateTime = this.toLocalDateTime(TimeZone.currentSystemDefault())
    val month = getMonthAbbreviation(localDateTime.month.number)
    return "$month ${localDateTime.day}, ${localDateTime.year}"
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
