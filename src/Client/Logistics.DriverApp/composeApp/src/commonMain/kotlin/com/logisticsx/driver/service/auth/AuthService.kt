package com.logisticsx.driver.service.auth

import com.logisticsx.driver.api.HttpClientFactory
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.util.decodeJwtPayload
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.forms.submitForm
import io.ktor.http.Parameters
import io.ktor.http.isSuccess
import kotlin.time.Clock


class AuthService(
    private val authorityUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private val tokenEndpoint = "${authorityUrl.trimEnd('/')}/connect/token"

    private val lazyHttpClient = lazy { HttpClientFactory.create() }
    private val httpClient: HttpClient by lazyHttpClient

    suspend fun login(username: String, password: String): Result<Unit> {
        return try {
            val response = httpClient.submitForm(
                url = tokenEndpoint,
                formParameters = Parameters.build {
                    append("grant_type", "password")
                    append("client_id", AuthConfig.CLIENT_ID)
                    append("username", username)
                    append("password", password)
                    append("scope", AuthConfig.SCOPE)
                }
            )

            if (response.status.isSuccess()) {
                val tokenResponse = response.body<TokenResponse>()
                saveTokens(tokenResponse)
                Result.success(Unit)
            } else {
                val errorResponse = try {
                    response.body<TokenErrorResponse>()
                } catch (e: Exception) {
                    TokenErrorResponse(
                        "unknown_error",
                        "Login failed with status ${response.status.value}"
                    )
                }
                Result.failure(
                    AuthException(
                        errorResponse.error,
                        errorResponse.errorDescription ?: "Authentication failed"
                    )
                )
            }
        } catch (e: Exception) {
            Result.failure(AuthException("network_error", e.message ?: "Network error occurred"))
        }
    }

    suspend fun isLoggedIn(): Boolean {
        val token = preferencesManager.getAccessToken()
        return token != null && !isTokenExpired()
    }

    suspend fun isTokenExpired(): Boolean {
        val expiry = preferencesManager.getTokenExpiry() ?: return true
        // Consider token expired 5 minutes before actual expiry
        return Clock.System.now().toEpochMilliseconds() > (expiry - 300000)
    }

    suspend fun refreshToken(): Result<Unit> {
        val refreshToken = preferencesManager.getRefreshToken()
            ?: return Result.failure(
                AuthException(
                    "no_refresh_token",
                    "No refresh token available"
                )
            )

        return try {
            val response = httpClient.submitForm(
                url = tokenEndpoint,
                formParameters = Parameters.build {
                    append("grant_type", "refresh_token")
                    append("client_id", AuthConfig.CLIENT_ID)
                    append("refresh_token", refreshToken)
                    append("scope", AuthConfig.SCOPE)
                }
            )

            if (response.status.isSuccess()) {
                val tokenResponse = response.body<TokenResponse>()
                saveTokens(tokenResponse)
                Result.success(Unit)
            } else {
                val errorResponse = try {
                    response.body<TokenErrorResponse>()
                } catch (e: Exception) {
                    TokenErrorResponse(
                        "unknown_error",
                        "Token refresh failed with status ${response.status.value}"
                    )
                }
                Result.failure(
                    AuthException(
                        errorResponse.error,
                        errorResponse.errorDescription ?: "Token refresh failed"
                    )
                )
            }
        } catch (e: Exception) {
            Result.failure(AuthException("network_error", e.message ?: "Network error occurred"))
        }
    }

    suspend fun logout() {
        preferencesManager.clearAll()
        close()
    }

    /**
     * Closes the underlying HttpClient to release resources.
     */
    fun close() {
        if (lazyHttpClient.isInitialized()) {
            httpClient.close()
        }
    }

    private suspend fun saveTokens(tokenResponse: TokenResponse) {
        preferencesManager.saveAccessToken(tokenResponse.accessToken)
        tokenResponse.refreshToken?.let { preferencesManager.saveRefreshToken(it) }
        tokenResponse.idToken?.let { preferencesManager.saveIdToken(it) }

        val expiryTime =
            Clock.System.now().toEpochMilliseconds() + (tokenResponse.expiresIn * 1000L)
        preferencesManager.saveTokenExpiry(expiryTime)

        // Extract user info from token
        extractAndSaveUserInfo(tokenResponse.accessToken)
    }

    private suspend fun extractAndSaveUserInfo(accessToken: String) {
        try {
            val payload = decodeJwtPayload(accessToken)

            payload["sub"]?.let { userId ->
                preferencesManager.saveUserId(userId)
            }

            payload["tenant"]?.let { tenantId ->
                preferencesManager.saveTenantId(tenantId)
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }
}
