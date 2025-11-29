package com.jfleets.driver.util

import platform.Foundation.NSLog

/**
 * iOS implementation using NSLog
 */
actual object Logger {
    actual fun d(tag: String, message: String) {
        NSLog("D/$tag: $message")
    }

    actual fun i(tag: String, message: String) {
        NSLog("I/$tag: $message")
    }

    actual fun w(tag: String, message: String) {
        NSLog("W/$tag: $message")
    }

    actual fun e(tag: String, message: String, throwable: Throwable?) {
        if (throwable != null) {
            NSLog("E/$tag: $message\n${throwable.stackTraceToString()}")
        } else {
            NSLog("E/$tag: $message")
        }
    }
}
