package com.jfleets.driver.data.dto

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

/**
 * Non-generic operation result without a payload.
 */
@Serializable
data class Result(
    @SerialName("error")
    val error: String? = null
) {
    @SerialName("success")
    val success: Boolean
        get() = error.isNullOrEmpty()

    companion object {
        fun ok(): Result = Result()
        fun fail(error: String): Result = Result(error = error)
    }
}

/**
 * Operation result that carries a payload of type [T] on success.
 * By convention, [data] is only populated for successful results.
 */
@Serializable
data class ApiResult<T>(
    @SerialName("data")
    val data: T? = null,
    @SerialName("error")
    val error: String? = null
) {
    @SerialName("success")
    val success: Boolean
        get() = error.isNullOrEmpty()

    companion object {
        fun <T> ok(result: T): ApiResult<T> = ApiResult(data = result)
        fun <T> fail(error: String): ApiResult<T> = ApiResult(error = error)
    }
}

/**
 * Paged result for list operations.
 */
@Serializable
data class PagedResult<T>(
    @SerialName("data")
    val data: List<T>? = null,
    @SerialName("totalItems")
    val totalItems: Int = 0,
    @SerialName("totalPages")
    val totalPages: Int = 0,
    @SerialName("error")
    val error: String? = null
) {
    @SerialName("success")
    val success: Boolean
        get() = error.isNullOrEmpty()

    companion object {
        fun <T> ok(items: List<T>?, totalItems: Int, totalPages: Int): PagedResult<T> =
            PagedResult(data = items, totalItems = totalItems, totalPages = totalPages)

        fun <T> fail(error: String): PagedResult<T> =
            PagedResult(error = error)
    }
}
