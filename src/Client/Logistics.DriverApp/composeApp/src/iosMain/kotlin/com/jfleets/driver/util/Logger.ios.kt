package com.jfleets.driver.util

import platform.Foundation.NSLog

/**
 * iOS implementation using NSLog.
 * Supports auto-discovery of TAG from calling class or function.
 */
actual object Logger {
    /**
     * Discovers the TAG from the call stack.
     * Returns the simple class name or function name of the caller.
     */
    private fun discoverTag(): String {
        val stackTrace = Throwable().stackTraceToString()
        val lines = stackTrace.lines()

        // Find the caller frame (skip Logger frames)
        for (line in lines) {
            if (line.contains("com.jfleets.driver") && !line.contains("Logger")) {
                // Extract class/function name from the stack trace line
                val match = Regex("""at\s+([^\s(]+)""").find(line)
                if (match != null) {
                    val fullName = match.groupValues[1]
                    return fullName.substringAfterLast('.')
                        .substringBefore('$')
                        .ifEmpty { "Unknown" }
                }
            }
        }
        return "Unknown"
    }

    actual fun d(tag: String, message: String) {
        NSLog("D/$tag: $message")
    }

    actual fun d(message: String) {
        NSLog("D/${discoverTag()}: $message")
    }

    actual fun i(tag: String, message: String) {
        NSLog("I/$tag: $message")
    }

    actual fun i(message: String) {
        NSLog("I/${discoverTag()}: $message")
    }

    actual fun w(tag: String, message: String) {
        NSLog("W/$tag: $message")
    }

    actual fun w(message: String) {
        NSLog("W/${discoverTag()}: $message")
    }

    actual fun e(tag: String, message: String, throwable: Throwable?) {
        if (throwable != null) {
            NSLog("E/$tag: $message\n${throwable.stackTraceToString()}")
        } else {
            NSLog("E/$tag: $message")
        }
    }

    actual fun e(message: String, throwable: Throwable?) {
        val tag = discoverTag()
        if (throwable != null) {
            NSLog("E/$tag: $message\n${throwable.stackTraceToString()}")
        } else {
            NSLog("E/$tag: $message")
        }
    }
}
