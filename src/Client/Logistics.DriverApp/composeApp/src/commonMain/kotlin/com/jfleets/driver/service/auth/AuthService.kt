package com.jfleets.driver.service.auth

import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.util.decodeJwtPayload
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.plugins.HttpTimeout
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.logging.DEFAULT
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.plugins.logging.Logging
import io.ktor.client.request.forms.submitForm
import io.ktor.client.request.header
import io.ktor.http.Parameters
import io.ktor.http.isSuccess
import io.ktor.serialization.kotlinx.json.json
import io.ktor.utils.io.charsets.Charsets
import io.ktor.utils.io.core.toByteArray
import kotlinx.serialization.json.Json
import kotlin.io.encoding.Base64
import kotlin.io.encoding.ExperimentalEncodingApi
import kotlin.time.Clock


class AuthService(
    private val authorityUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private val allowSelfSigned = authorityUrl.contains("10.0.2.2")
    private val tokenEndpoint = "${authorityUrl.trimEnd('/')}/connect/token"

    private val httpClient: HttpClient by lazy { createHttpClient() }

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
        }
    }

    @OptIn(ExperimentalEncodingApi::class)
    private fun createBasicAuthHeader(): String {
        val credentials = "${AuthConfig.CLIENT_ID}:${AuthConfig.CLIENT_SECRET}"
        val encoded = Base64.encode(credentials.toByteArray(Charsets.UTF_8))
        return "Basic $encoded"
    }

    suspend fun login(username: String, password: String): Result<Unit> {

        return try {
            val response = httpClient.submitForm(
                url = tokenEndpoint,
                formParameters = Parameters.build {
                    append("grant_type", "password")
                    append("username", username)
                    append("password", password)
                    append("scope", AuthConfig.SCOPE)
                }
            ) {
                header("Authorization", createBasicAuthHeader())
            }

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
                    append("refresh_token", refreshToken)
                    append("scope", AuthConfig.SCOPE)
                }
            ) {
                header("Authorization", createBasicAuthHeader())
            }

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
