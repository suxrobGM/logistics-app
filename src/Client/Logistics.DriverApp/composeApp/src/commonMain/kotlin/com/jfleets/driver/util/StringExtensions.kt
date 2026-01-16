package com.jfleets.driver.util

/**
 * Extracts initials from a name string.
 * Takes the first letter of the first two words.
 * e.g., "John Doe" -> "JD", "Alice" -> "A"
 */
fun String.getInitials(): String {
    return this
        .split(" ")
        .take(2)
        .mapNotNull { it.firstOrNull()?.uppercase() }
        .joinToString("")
        .ifEmpty { "?" }
}

/**
 * Formats a list of names as a comma-separated string.
 * Returns "Unknown" if the list is empty or null.
 */
fun List<String?>?.formatAsNames(): String {
    return this
        ?.filterNotNull()
        ?.filter { it.isNotBlank() }
        ?.joinToString(", ")
        ?.ifEmpty { "Unknown" }
        ?: "Unknown"
}
