package com.jfleets.driver.util

/**
 * Multiplatform logging utility.
 * Uses platform-specific implementations:
 * - Android: android.util.Log
 * - iOS: NSLog / print
 *
 * Supports auto-discovery of TAG from calling class or function.
 */
expect object Logger {
    /** Log debug message with explicit tag */
    fun d(tag: String, message: String)

    /** Log debug message with auto-discovered tag */
    fun d(message: String)

    /** Log info message with explicit tag */
    fun i(tag: String, message: String)

    /** Log info message with auto-discovered tag */
    fun i(message: String)

    /** Log warning message with explicit tag */
    fun w(tag: String, message: String)

    /** Log warning message with auto-discovered tag */
    fun w(message: String)

    /** Log error message with explicit tag */
    fun e(tag: String, message: String, throwable: Throwable? = null)

    /** Log error message with auto-discovered tag */
    fun e(message: String, throwable: Throwable? = null)
}
