package com.jfleets.driver.util

/**
 * Returns the current time in milliseconds since epoch.
 * Platform-specific implementations:
 * - Android: System.currentTimeMillis()
 * - iOS: NSDate().timeIntervalSince1970 * 1000
 * @return Current time in milliseconds
 */
expect fun currentTimeMillis(): Long
