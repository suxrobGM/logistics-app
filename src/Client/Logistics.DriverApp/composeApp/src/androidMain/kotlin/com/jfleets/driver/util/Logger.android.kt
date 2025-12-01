package com.jfleets.driver.util

import android.util.Log

/**
 * Android implementation using android.util.Log.
 * Supports auto-discovery of TAG from calling class or function.
 */
actual object Logger {
    /**
     * Discovers the TAG from the call stack.
     * Returns the simple class name or function name of the caller.
     */
    private fun discoverTag(): String {
        val stackTrace = Throwable().stackTrace
        // Skip: [0] discoverTag, [1] Logger method, [2] actual caller
        val callerFrame = stackTrace.getOrNull(2) ?: return "Unknown"

        val className = callerFrame.className
        val simpleClassName = className.substringAfterLast('.')
            .substringBefore('$') // Handle inner classes and lambdas

        return if (simpleClassName.isNotEmpty() && simpleClassName != "Companion") {
            simpleClassName
        } else {
            callerFrame.methodName
        }
    }

    actual fun d(tag: String, message: String) {
        Log.d(tag, message)
    }

    actual fun d(message: String) {
        Log.d(discoverTag(), message)
    }

    actual fun i(tag: String, message: String) {
        Log.i(tag, message)
    }

    actual fun i(message: String) {
        Log.i(discoverTag(), message)
    }

    actual fun w(tag: String, message: String) {
        Log.w(tag, message)
    }

    actual fun w(message: String) {
        Log.w(discoverTag(), message)
    }

    actual fun e(tag: String, message: String, throwable: Throwable?) {
        if (throwable != null) {
            Log.e(tag, message, throwable)
        } else {
            Log.e(tag, message)
        }
    }

    actual fun e(message: String, throwable: Throwable?) {
        val tag = discoverTag()
        if (throwable != null) {
            Log.e(tag, message, throwable)
        } else {
            Log.e(tag, message)
        }
    }
}
