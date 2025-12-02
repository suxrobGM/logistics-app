package com.jfleets.driver.util

import java.text.NumberFormat
import java.util.Locale

actual fun Double.formatCurrency(): String {
    val locale = Locale.getDefault()
    val format = NumberFormat.getCurrencyInstance(locale)
    return format.format(this)
}
