package com.jfleets.driver.data.api

/**
 * Represents a non-success HTTP response from the backend.
 */
class ApiException(
    val statusCode: Int,
    val endpoint: String,
    val responseBody: String? = null
) : Exception(
    buildString {
        append("Request to '")
        append(endpoint)
        append("' failed with status ")
        append(statusCode)
        responseBody?.takeIf { it.isNotBlank() }?.let {
            append(": ")
            append(it)
        }
    }
)
