package com.jfleets.driver

import kotlinx.datetime.TimeZone
import kotlinx.datetime.toLocalDateTime
import java.time.format.DateTimeFormatter
import kotlin.time.Instant

actual fun Instant.formatShort(): String {
    this.toLocalDateTime(TimeZone.currentSystemDefault()).date
    val javaInstant =
        java.time.Instant.ofEpochSecond(this.epochSeconds, this.nanosecondsOfSecond.toLong())
    val formatter = DateTimeFormatter.ofPattern("MMM dd, yyyy")
    return formatter.format(javaInstant)
}

actual fun Instant.formatDateTime(): String {
    val javaInstant =
        java.time.Instant.ofEpochSecond(this.epochSeconds, this.nanosecondsOfSecond.toLong())
    val formatter = DateTimeFormatter.ofPattern("MMM dd, yyyy HH:mm")
    return formatter.format(javaInstant)
}
