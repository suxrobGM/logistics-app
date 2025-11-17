package com.jfleets.driver.data.auth

import android.content.Context
import android.content.Intent
import android.net.Uri
import net.openid.appauth.*
import timber.log.Timber
import kotlin.coroutines.resume
import kotlin.coroutines.resumeWithException
import kotlin.coroutines.suspendCoroutine

class AuthService(
    private val context: Context
) {
    companion object {
        private const val AUTHORITY = "https://10.0.2.2:7001"
        private const val CLIENT_ID = "logistics.driverapp"
        private const val CLIENT_SECRET = "Super secret key 2"
        private const val REDIRECT_URI = "logistics-driver://callback"
        private const val SCOPE = "openid profile offline_access roles tenant logistics.api.tenant"
    }

    private val serviceConfig = AuthorizationServiceConfiguration(
        Uri.parse("$AUTHORITY/connect/authorize"),
        Uri.parse("$AUTHORITY/connect/token")
    )

    private val authService = AuthorizationService(context)

    fun getAuthorizationIntent(): Intent {
        val authRequestBuilder = AuthorizationRequest.Builder(
            serviceConfig,
            CLIENT_ID,
            ResponseTypeValues.CODE,
            Uri.parse(REDIRECT_URI)
        ).setScope(SCOPE)

        val authRequest = authRequestBuilder.build()
        return authService.getAuthorizationRequestIntent(authRequest)
    }

    suspend fun handleAuthorizationResponse(intent: Intent): AuthResult = suspendCoroutine { continuation ->
        val response = AuthorizationResponse.fromIntent(intent)
        val exception = AuthorizationException.fromIntent(intent)

        when {
            response != null -> {
                // Exchange authorization code for tokens
                val tokenRequest = response.createTokenExchangeRequest()
                authService.performTokenRequest(tokenRequest) { tokenResponse, tokenException ->
                    when {
                        tokenResponse != null -> {
                            val result = AuthResult(
                                accessToken = tokenResponse.accessToken ?: "",
                                refreshToken = tokenResponse.refreshToken,
                                idToken = tokenResponse.idToken,
                                expiresIn = tokenResponse.accessTokenExpirationTime?.let {
                                    ((it - System.currentTimeMillis()) / 1000).toInt()
                                } ?: 3600
                            )
                            continuation.resume(result)
                        }
                        tokenException != null -> {
                            Timber.e(tokenException, "Token exchange failed")
                            continuation.resumeWithException(
                                Exception("Token exchange failed: ${tokenException.message}")
                            )
                        }
                        else -> {
                            continuation.resumeWithException(
                                Exception("Unknown error during token exchange")
                            )
                        }
                    }
                }
            }
            exception != null -> {
                Timber.e(exception, "Authorization failed")
                continuation.resumeWithException(
                    Exception("Authorization failed: ${exception.message}")
                )
            }
            else -> {
                continuation.resumeWithException(
                    Exception("No authorization response or exception found")
                )
            }
        }
    }

    suspend fun refreshAccessToken(refreshToken: String): AuthResult = suspendCoroutine { continuation ->
        val tokenRequest = TokenRequest.Builder(
            serviceConfig,
            CLIENT_ID
        )
            .setGrantType(GrantTypeValues.REFRESH_TOKEN)
            .setRefreshToken(refreshToken)
            .setScope(SCOPE)
            .build()

        val clientAuth = ClientSecretBasic(CLIENT_SECRET)

        authService.performTokenRequest(tokenRequest, clientAuth) { tokenResponse, exception ->
            when {
                tokenResponse != null -> {
                    val result = AuthResult(
                        accessToken = tokenResponse.accessToken ?: "",
                        refreshToken = tokenResponse.refreshToken ?: refreshToken,
                        idToken = tokenResponse.idToken,
                        expiresIn = tokenResponse.accessTokenExpirationTime?.let {
                            ((it - System.currentTimeMillis()) / 1000).toInt()
                        } ?: 3600
                    )
                    continuation.resume(result)
                }
                exception != null -> {
                    Timber.e(exception, "Token refresh failed")
                    continuation.resumeWithException(
                        Exception("Token refresh failed: ${exception.message}")
                    )
                }
                else -> {
                    continuation.resumeWithException(
                        Exception("Unknown error during token refresh")
                    )
                }
            }
        }
    }

    fun dispose() {
        authService.dispose()
    }
}

data class AuthResult(
    val accessToken: String,
    val refreshToken: String?,
    val idToken: String?,
    val expiresIn: Int
)
