package com.jfleets.driver.data.auth

import com.jfleets.driver.data.api.createPlatformHttpClient
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
import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import kotlin.io.encoding.Base64
import kotlin.io.encoding.ExperimentalEncodingApi

@Serializable
data class TokenResponse(
    @SerialName("access_token") val accessToken: String,
    @SerialName("refresh_token") val refreshToken: String? = null,
    @SerialName("id_token") val idToken: String? = null,
    @SerialName("expires_in") val expiresIn: Int,
    @SerialName("token_type") val tokenType: String
)

@Serializable
data class TokenErrorResponse(
    val error: String,
    @SerialName("error_description") val errorDescription: String? = null
)

class OAuthService(
    private val authorityUrl: String
) {
    companion object {
        private const val CLIENT_ID = "logistics.driverapp"
        private const val CLIENT_SECRET = "Super secret key 2"
        private const val SCOPE = "openid profile offline_access roles tenant logistics.api.tenant"
    }

    private val allowSelfSigned = authorityUrl.contains("10.0.2.2")
    private val tokenEndpoint = "${authorityUrl.trimEnd('/')}/connect/token"

    private val httpClient: HttpClient by lazy { createHttpClient() }

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
        }
    }

    @OptIn(ExperimentalEncodingApi::class)
    private fun createBasicAuthHeader(): String {
        val credentials = "$CLIENT_ID:$CLIENT_SECRET"
        val encoded = Base64.encode(credentials.toByteArray(Charsets.UTF_8))
        return "Basic $encoded"
    }

    suspend fun login(username: String, password: String): Result<TokenResponse> {
        return try {
            val response = httpClient.submitForm(
                url = tokenEndpoint,
                formParameters = Parameters.build {
                    append("grant_type", "password")
                    append("username", username)
                    append("password", password)
                    append("scope", SCOPE)
                }
            ) {
                header("Authorization", createBasicAuthHeader())
            }

            if (response.status.isSuccess()) {
                Result.success(response.body<TokenResponse>())
            } else {
                val errorResponse = try {
                    response.body<TokenErrorResponse>()
                } catch (e: Exception) {
                    TokenErrorResponse("unknown_error", "Login failed with status ${response.status.value}")
                }
                Result.failure(
                    OAuthException(
                        errorResponse.error,
                        errorResponse.errorDescription ?: "Authentication failed"
                    )
                )
            }
        } catch (e: Exception) {
            Result.failure(OAuthException("network_error", e.message ?: "Network error occurred"))
        }
    }

    suspend fun refreshToken(refreshToken: String): Result<TokenResponse> {
        return try {
            val response = httpClient.submitForm(
                url = tokenEndpoint,
                formParameters = Parameters.build {
                    append("grant_type", "refresh_token")
                    append("refresh_token", refreshToken)
                    append("scope", SCOPE)
                }
            ) {
                header("Authorization", createBasicAuthHeader())
            }

            if (response.status.isSuccess()) {
                Result.success(response.body<TokenResponse>())
            } else {
                val errorResponse = try {
                    response.body<TokenErrorResponse>()
                } catch (e: Exception) {
                    TokenErrorResponse("unknown_error", "Token refresh failed with status ${response.status.value}")
                }
                Result.failure(
                    OAuthException(
                        errorResponse.error,
                        errorResponse.errorDescription ?: "Token refresh failed"
                    )
                )
            }
        } catch (e: Exception) {
            Result.failure(OAuthException("network_error", e.message ?: "Network error occurred"))
        }
    }
}

class OAuthException(
    val error: String,
    override val message: String
) : Exception(message)
