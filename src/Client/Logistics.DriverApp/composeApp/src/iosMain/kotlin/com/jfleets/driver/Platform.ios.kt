package com.jfleets.driver

import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.datetime.toNSDate
import platform.Foundation.NSDateFormatter
import platform.Foundation.NSDateFormatterMediumStyle
import platform.Foundation.NSDateFormatterNoStyle
import platform.Foundation.NSDateFormatterShortStyle
import kotlin.time.Instant

@OptIn(ExperimentalForeignApi::class)
actual fun Instant.formatShort(): String {
    val nsDate = this.toNSDate()
    val formatter = NSDateFormatter()
    formatter.dateStyle = NSDateFormatterMediumStyle
    formatter.timeStyle = NSDateFormatterNoStyle
    return formatter.stringFromDate(nsDate)
}

@OptIn(ExperimentalForeignApi::class)
actual fun Instant.formatDateTime(): String {
    val nsDate = this.toNSDate()
    val formatter = NSDateFormatter()
    formatter.dateStyle = NSDateFormatterMediumStyle
    formatter.timeStyle = NSDateFormatterShortStyle
    return formatter.stringFromDate(nsDate)
}
