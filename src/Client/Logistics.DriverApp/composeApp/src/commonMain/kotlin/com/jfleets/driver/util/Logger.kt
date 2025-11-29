package com.jfleets.driver.util

/**
 * Multiplatform logging utility.
 * Uses platform-specific implementations:
 * - Android: android.util.Log
 * - iOS: NSLog / print
 */
expect object Logger {
    fun d(tag: String, message: String)
    fun i(tag: String, message: String)
    fun w(tag: String, message: String)
    fun e(tag: String, message: String, throwable: Throwable? = null)
}

/**
 * Extension to log with a default tag based on class name.
 * Usage: myObject.logD("Debug message")
 */
inline fun <reified T> T.logD(message: String) {
    Logger.d(T::class.simpleName ?: "Unknown", message)
}

inline fun <reified T> T.logI(message: String) {
    Logger.i(T::class.simpleName ?: "Unknown", message)
}

inline fun <reified T> T.logW(message: String) {
    Logger.w(T::class.simpleName ?: "Unknown", message)
}

inline fun <reified T> T.logE(message: String, throwable: Throwable? = null) {
    Logger.e(T::class.simpleName ?: "Unknown", message, throwable)
}
