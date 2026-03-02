package com.logisticsx.driver.api

import com.logisticsx.driver.infrastructure.HttpResponse

/**
 * Exception thrown when an API call returns a non-success status code.
 */
class ApiException(
    val statusCode: Int,
    override val message: String = "API request failed with status $statusCode"
) : Exception(message)

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
