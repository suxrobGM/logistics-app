package com.jfleets.driver.data.repository

import android.content.Intent
import com.jfleets.driver.data.auth.AuthResult
import com.jfleets.driver.data.auth.AuthService
import com.jfleets.driver.data.local.TokenManager
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

/**
 * Android implementation of AuthRepository.
 * Uses AppAuth for OAuth 2.0 authentication with Intent-based flow.
 */
class AndroidAuthRepository(
    private val authService: AuthService,
    private val tokenManager: TokenManager
) : AuthRepository {

    /**
     * Android-specific: Creates an Intent to launch the OAuth login flow.
     */
    fun getLoginIntent(): Intent {
        return authService.getAuthorizationIntent()
    }

    /**
     * Android-specific: Handles the OAuth callback Intent.
     */
    suspend fun handleAuthorizationResponse(intent: Intent): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            val authResult = authService.handleAuthorizationResponse(intent)
            saveAuthResult(authResult)

            // Fetch tenant ID
            fetchAndSaveTenantId()

            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    override suspend fun refreshToken(): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            val refreshToken = tokenManager.getRefreshToken()
                ?: return@withContext Result.failure(Exception("No refresh token available"))

            val authResult = authService.refreshAccessToken(refreshToken)
            saveAuthResult(authResult)

            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    override suspend fun logout() = withContext(Dispatchers.IO) {
        tokenManager.clearTokens()
    }

    override suspend fun isLoggedIn(): Boolean = withContext(Dispatchers.IO) {
        val token = tokenManager.getAccessToken()
        token != null && !tokenManager.isTokenExpired()
    }

    override suspend fun isTokenExpired(): Boolean = withContext(Dispatchers.IO) {
        tokenManager.isTokenExpired()
    }

    private suspend fun saveAuthResult(authResult: AuthResult) {
        tokenManager.saveTokens(
            accessToken = authResult.accessToken,
            refreshToken = authResult.refreshToken,
            idToken = authResult.idToken,
            expiresIn = authResult.expiresIn
        )
    }

    private suspend fun fetchAndSaveTenantId() {
        // Tenant ID is already extracted from the JWT token in TokenManager
    }
}
