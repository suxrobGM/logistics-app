package com.jfleets.driver.data.api

import io.ktor.client.call.body
import io.ktor.client.plugins.HttpTimeout
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.defaultRequest
import io.ktor.client.plugins.logging.DEFAULT
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.plugins.logging.Logging
import io.ktor.client.request.HttpRequestBuilder
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.post
import io.ktor.client.request.put
import io.ktor.client.request.setBody
import io.ktor.client.statement.HttpResponse
import io.ktor.client.statement.bodyAsText
import io.ktor.http.ContentType
import io.ktor.http.contentType
import io.ktor.http.isSuccess
import io.ktor.serialization.kotlinx.json.json
import kotlinx.serialization.json.Json

class ApiClient(
    private val baseUrl: String,
    @PublishedApi
    internal val getAccessToken: suspend () -> String?,
    @PublishedApi
    internal val getTenantId: suspend () -> String?
) {
    private val allowSelfSigned = baseUrl.contains("10.0.2.2")

    val client = createPlatformHttpClient(allowSelfSigned) {
        install(ContentNegotiation) {
            json(Json {
                prettyPrint = true
                isLenient = true
                ignoreUnknownKeys = true
            })
        }

        install(Logging) {
            logger = Logger.DEFAULT
            level = LogLevel.INFO
        }

        install(HttpTimeout) {
            requestTimeoutMillis = 30000
            connectTimeoutMillis = 30000
        }

        defaultRequest {
            url(baseUrl)
            contentType(ContentType.Application.Json)
        }
    }

    suspend inline fun <reified T> get(
        endpoint: String,
        noinline block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        val response = client.get(endpoint) {
            if (token != null) {
                header("Authorization", "Bearer $token")
            }
            if (tenantId != null) {
                header("X-Tenant", tenantId)
            }
            block()
        }
        return response.requireSuccess(endpoint)
    }

    suspend inline fun <reified T> post(
        endpoint: String,
        body: Any? = null,
        noinline block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        val response = client.post(endpoint) {
            if (token != null) {
                header("Authorization", "Bearer $token")
            }
            if (tenantId != null) {
                header("X-Tenant", tenantId)
            }
            if (body != null) {
                setBody(body)
            }
            block()
        }
        return response.requireSuccess(endpoint)
    }

    suspend inline fun <reified T> put(
        endpoint: String,
        body: Any? = null,
        noinline block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        val response = client.put(endpoint) {
            if (token != null) {
                header("Authorization", "Bearer $token")
            }
            if (tenantId != null) {
                header("X-Tenant", tenantId)
            }
            if (body != null) {
                setBody(body)
            }
            block()
        }
        return response.requireSuccess(endpoint)
    }
}

suspend inline fun <reified T> HttpResponse.requireSuccess(endpoint: String): T {
    if (!status.isSuccess()) {
        val errorText = runCatching { bodyAsText() }.getOrElse { "" }
        throw ApiException(status.value, endpoint, errorText)
    }
    return body()
}
