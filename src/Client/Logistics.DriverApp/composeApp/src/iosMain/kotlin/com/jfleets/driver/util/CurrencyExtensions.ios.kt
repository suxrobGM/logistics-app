package com.jfleets.driver.util

import platform.Foundation.NSLocale
import platform.Foundation.NSNumber
import platform.Foundation.NSNumberFormatter
import platform.Foundation.NSNumberFormatterCurrencyStyle
import platform.Foundation.autoupdatingCurrentLocale

actual fun Double.formatCurrency(): String {
    val formatter = NSNumberFormatter()
    val locale = NSLocale.autoupdatingCurrentLocale
    formatter.numberStyle = NSNumberFormatterCurrencyStyle
    formatter.locale = locale
    return formatter.stringFromNumber(NSNumber(this)) ?: "$${this}"
}
