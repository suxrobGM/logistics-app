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

/**
 * Factory for API clients generated from OpenAPI spec.
 *
 * The API clients (LoadApi, EmployeeApi, etc.) are auto-generated from the
 * backend's swagger.json.
 *
 * Generated APIs are in: com.jfleets.driver.api
 * Generated models (DTOs) are in: com.jfleets.driver.api.models
 */
class ApiFactory(
    private val baseUrl: String,
    private val preferencesManager: PreferencesManager
) {

    val httpClient: HttpClient by lazy { createHttpClient() }

    // Generated API clients (from OpenAPI spec)
    val customerApi: CustomerApi by lazy { CustomerApi(baseUrl, httpClient) }
    val documentApi: DocumentApi by lazy { DocumentApi(baseUrl, httpClient) }
    val driverApi: DriverApi by lazy { DriverApi(baseUrl, httpClient) }
    val employeeApi: EmployeeApi by lazy { EmployeeApi(baseUrl, httpClient) }
    val inspectionApi: InspectionApi by lazy { InspectionApi(baseUrl, httpClient) }
    val loadApi: LoadApi by lazy { LoadApi(baseUrl, httpClient) }
    val messageApi: MessageApi by lazy { MessageApi(baseUrl, httpClient) }
    val reportApi: ReportApi by lazy { ReportApi(baseUrl, httpClient) }
    val truckApi: TruckApi by lazy { TruckApi(baseUrl, httpClient) }

    private fun createHttpClient(): HttpClient {
        return HttpClient {
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
