package com.logisticsx.driver.api

import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.auth.AuthEventBus
import com.logisticsx.driver.service.auth.AuthService
import com.logisticsx.driver.util.Logger
import io.ktor.client.HttpClient
import io.ktor.client.plugins.api.createClientPlugin
import io.ktor.client.plugins.auth.Auth
import io.ktor.client.plugins.auth.providers.BearerTokens
import io.ktor.client.plugins.auth.providers.bearer
import io.ktor.client.plugins.defaultRequest
import io.ktor.client.request.header
import io.ktor.http.ContentType
import io.ktor.http.contentType

/**
 * Builds the API clients that `openApiGenerate` produces from the backend's OpenAPI spec.
 *
 * Regenerate those clients rather than editing them.
 */
class ApiFactory(
    private val baseUrl: String,
    private val preferencesManager: PreferencesManager,
    private val authService: AuthService
) {

    val httpClient: HttpClient by lazy { createHttpClient() }

    val customerApi: CustomerApi by lazy { CustomerApi(baseUrl, httpClient) }
    val documentApi: DocumentApi by lazy { DocumentApi(baseUrl, httpClient) }
    val driverApi: DriverApi by lazy { DriverApi(baseUrl, httpClient) }
    val dvirApi: DvirApi by lazy { DvirApi(baseUrl, httpClient) }
    val employeeApi: EmployeeApi by lazy { EmployeeApi(baseUrl, httpClient) }
    val inspectionsApi: InspectionsApi by lazy { InspectionsApi(baseUrl, httpClient) }
    val loadApi: LoadApi by lazy { LoadApi(baseUrl, httpClient) }
    val messageApi: MessageApi by lazy { MessageApi(baseUrl, httpClient) }
    val privacyApi: PrivacyApi by lazy { PrivacyApi(baseUrl, httpClient) }
    val reportApi: ReportApi by lazy { ReportApi(baseUrl, httpClient) }
    val statApi: StatApi by lazy { StatApi(baseUrl, httpClient) }
    val tripApi: TripApi by lazy { TripApi(baseUrl, httpClient) }
    val truckApi: TruckApi by lazy { TruckApi(baseUrl, httpClient) }
    val userApi: UserApi by lazy { UserApi(baseUrl, httpClient) }
    val vinsApi: VinsApi by lazy { VinsApi(baseUrl, httpClient) }

    private fun createHttpClient(): HttpClient = HttpClientFactory.create {
        defaultRequest {
            url(baseUrl)
            contentType(ContentType.Application.Json)
        }

        install(tenantHeaderPlugin(preferencesManager))

        install(Auth) {
            bearer {
                // DataStore owns the tokens, so a cached copy would survive a re-login and
                // authenticate the new driver as the previous one.
                cacheTokens = false

                // A screen being torn down must not cancel a refresh: the server may already have
                // rotated the refresh token, and losing the response would end the session for good.
                nonCancellableRefresh = true

                loadTokens { bearerTokens() }

                refreshTokens {
                    // Refresh goes through AuthService, which owns a separate HttpClient. Using
                    // this one would re-enter the plugin and deadlock.
                    if (authService.refreshToken().isFailure) {
                        Logger.e("ApiFactory: Token refresh failed, emitting unauthorized")
                        AuthEventBus.emitUnauthorized()
                        return@refreshTokens null
                    }
                    Logger.d("ApiFactory: Token refreshed, retrying request")
                    bearerTokens()
                }

                // Must stay true: the plugin only tracks a request's token version for providers
                // that authenticate preemptively, and that version is what suppresses duplicate
                // refreshes when several calls get a 401 at once.
                sendWithoutRequest { true }
            }
        }
    }

    private suspend fun bearerTokens(): BearerTokens? =
        preferencesManager.getAccessToken()?.let { accessToken ->
            BearerTokens(accessToken, preferencesManager.getRefreshToken())
        }
}

/**
 * Adds the `X-Tenant` header every API call needs.
 *
 * This cannot live in `defaultRequest`, whose block is not a suspend context while the tenant id
 * comes from DataStore. `onRequest` runs once per call, before the send pipeline, so a 401 retry
 * inherits the header from the original request builder.
 */
private fun tenantHeaderPlugin(preferencesManager: PreferencesManager) =
    createClientPlugin("TenantHeader") {
        onRequest { request, _ ->
            preferencesManager.getTenantId()?.let { request.header("X-Tenant", it) }
        }
    }
