package com.jfleets.driver

import kotlin.time.Instant

// Formatting utilities
expect fun Instant.formatShort(): String
expect fun Instant.formatDateTime(): String
