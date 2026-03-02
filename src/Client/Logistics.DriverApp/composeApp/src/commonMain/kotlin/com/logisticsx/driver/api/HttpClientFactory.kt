package com.logisticsx.driver.api

import io.ktor.client.HttpClient
import io.ktor.client.HttpClientConfig
import io.ktor.client.plugins.HttpTimeout
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.logging.DEFAULT
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.plugins.logging.Logging
import io.ktor.serialization.kotlinx.json.json
import kotlinx.serialization.json.Json

/**
 * Shared HttpClient configuration used by both ApiFactory and AuthService.
 * Avoids duplicating JSON, logging, and timeout setup.
 */
object HttpClientFactory {

    val jsonConfig = Json {
        prettyPrint = true
        isLenient = true
        ignoreUnknownKeys = true
    }

    /**
     * Creates an HttpClient with standard JSON, logging, and timeout config.
     * Additional configuration can be applied via the [block] parameter.
     */
    fun create(block: HttpClientConfig<*>.() -> Unit = {}): HttpClient {
        return HttpClient {
            install(ContentNegotiation) {
                json(jsonConfig)
            }

            install(Logging) {
                logger = Logger.DEFAULT
                level = LogLevel.INFO
            }

            install(HttpTimeout) {
                requestTimeoutMillis = 30000
                connectTimeoutMillis = 30000
            }

            block()
        }
    }
}
