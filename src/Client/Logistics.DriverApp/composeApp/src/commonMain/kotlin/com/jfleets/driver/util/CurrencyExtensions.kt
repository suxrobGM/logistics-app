package com.jfleets.driver.util

/**
 * Formats a Double as a currency string with the specified currency symbol.
 * The actual implementation is platform-specific.
 * @return The formatted currency string.
 */
expect fun Double.formatCurrency(): String
