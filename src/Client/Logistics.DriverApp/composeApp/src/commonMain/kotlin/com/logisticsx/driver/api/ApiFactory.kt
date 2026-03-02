package com.logisticsx.driver.api

import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.auth.AuthEventBus
import com.logisticsx.driver.service.auth.AuthService
import com.logisticsx.driver.util.Logger
import io.ktor.client.HttpClient
import io.ktor.client.plugins.HttpSend
import io.ktor.client.plugins.defaultRequest
import io.ktor.client.plugins.plugin
import io.ktor.client.request.header
import io.ktor.http.ContentType
import io.ktor.http.HttpStatusCode
import io.ktor.http.contentType

/**
 * Factory for API clients generated from OpenAPI spec.
 *
 * The API clients (LoadApi, EmployeeApi, etc.) are auto-generated from the
 * backend's swagger.json.
 *
 * Generated APIs are in: com.logisticsx.driver.api
 * Generated models (DTOs) are in: com.logisticsx.driver.api.models
 */
class ApiFactory(
    private val baseUrl: String,
    private val preferencesManager: PreferencesManager,
    private val authService: AuthService
) {

    val httpClient: HttpClient by lazy { createHttpClient() }

    // Generated API clients (from OpenAPI spec)
    val customerApi: CustomerApi by lazy { CustomerApi(baseUrl, httpClient) }
    val documentApi: DocumentApi by lazy { DocumentApi(baseUrl, httpClient) }
    val driverApi: DriverApi by lazy { DriverApi(baseUrl, httpClient) }
    val dvirApi: DvirApi by lazy { DvirApi(baseUrl, httpClient) }
    val employeeApi: EmployeeApi by lazy { EmployeeApi(baseUrl, httpClient) }
    val inspectionApi: InspectionApi by lazy { InspectionApi(baseUrl, httpClient) }
    val loadApi: LoadApi by lazy { LoadApi(baseUrl, httpClient) }
    val messageApi: MessageApi by lazy { MessageApi(baseUrl, httpClient) }
    val reportApi: ReportApi by lazy { ReportApi(baseUrl, httpClient) }
    val statApi: StatApi by lazy { StatApi(baseUrl, httpClient) }
    val tripApi: TripApi by lazy { TripApi(baseUrl, httpClient) }
    val truckApi: TruckApi by lazy { TruckApi(baseUrl, httpClient) }
    val userApi: UserApi by lazy { UserApi(baseUrl, httpClient) }

    private fun createHttpClient(): HttpClient {
        val client = HttpClientFactory.create {
            defaultRequest {
                url(baseUrl)
                contentType(ContentType.Application.Json)
            }
        }

        // Intercept requests to inject auth headers and handle token refresh
        client.plugin(HttpSend).intercept { request ->
            preferencesManager.getAccessToken()?.let { token ->
                request.header("Authorization", "Bearer $token")
            }
            preferencesManager.getTenantId()?.let { tenantId ->
                request.header("X-Tenant", tenantId)
            }

            val originalCall = execute(request)

            // On 401, attempt token refresh and retry once
            if (originalCall.response.status == HttpStatusCode.Unauthorized) {
                val refreshResult = authService.refreshToken()
                if (refreshResult.isSuccess) {
                    Logger.d("ApiFactory: Token refreshed, retrying request")
                    // Retry with new token
                    val newToken = preferencesManager.getAccessToken()
                    request.headers.remove("Authorization")
                    newToken?.let { request.header("Authorization", "Bearer $it") }
                    execute(request)
                } else {
                    Logger.e("ApiFactory: Token refresh failed, emitting unauthorized")
                    AuthEventBus.emitUnauthorized()
                    originalCall
                }
            } else {
                originalCall
            }
        }

        return client
    }
}
