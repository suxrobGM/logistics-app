package com.logisticsx.driver.api

import com.logisticsx.driver.infrastructure.HttpResponse

/**
 * Exception thrown when an API call returns a non-success status code.
 *
 * [message] is surfaced verbatim by `BaseViewModel` error states, so the default is written
 * for a driver reading it full-screen, not for a log line.
 */
class ApiException(
    val statusCode: Int,
    override val message: String = messageForStatus(statusCode)
) : Exception(message)

private fun messageForStatus(statusCode: Int): String = when (statusCode) {
    400, 422 -> "That request wasn't accepted. Check the details and try again."
    401 -> "Your session has expired. Please sign in again."
    403 -> "You don't have permission to do this. Your account's role may need updating in the TMS portal."
    404 -> "We couldn't find that. It may have been removed or reassigned."
    409 -> "This was already changed somewhere else. Refresh and try again."
    429 -> "Too many requests. Wait a moment and try again."
    in 500..599 -> "Something went wrong on our end. Try again in a moment."
    else -> "The request failed (status $statusCode). Try again."
}

/**
 * Extracts the body from an [HttpResponse], throwing [ApiException] if the response
 * indicates failure. Use this in repositories to ensure consistent error handling.
 */
suspend fun <T : Any> HttpResponse<T>.bodyOrThrow(): T {
    if (!success) {
        throw ApiException(status)
    }
    return body()
}
