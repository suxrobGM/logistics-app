package com.jfleets.driver.api

import com.jfleets.driver.service.PreferencesManager
import io.ktor.client.HttpClient
import io.ktor.client.plugins.HttpTimeout
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.defaultRequest
import io.ktor.client.plugins.logging.DEFAULT
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.plugins.logging.Logging
import io.ktor.client.request.header
import io.ktor.http.ContentType
import io.ktor.http.contentType
import io.ktor.serialization.kotlinx.json.json
import kotlinx.coroutines.runBlocking
import kotlinx.serialization.json.Json

class ApiFactory(
    private val baseUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private val allowSelfSigned = baseUrl.contains("10.0.2.2")

    private val httpClient: HttpClient by lazy { createHttpClient() }

    val loadApi: LoadApi by lazy { LoadApi(baseUrl, httpClient) }
    val truckApi: TruckApi by lazy { TruckApi(baseUrl, httpClient) }
    val userApi: UserApi by lazy { UserApi(baseUrl, httpClient) }
    val driverApi: DriverApi by lazy { DriverApi(baseUrl, httpClient) }
    val statApi: StatApi by lazy { StatApi(baseUrl, httpClient) }

    private fun createHttpClient(): HttpClient {
        return createPlatformHttpClient(allowSelfSigned) {
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

                runBlocking {
                    preferencesManager.getAccessToken()?.let { token ->
                        header("Authorization", "Bearer $token")
                    }
                    preferencesManager.getTenantId()?.let { tenantId ->
                        header("X-Tenant", tenantId)
                    }
                }
            }
        }
    }
}
