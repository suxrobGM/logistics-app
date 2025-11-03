package com.jfleets.driver.data.repository

import android.content.Intent
import com.jfleets.driver.data.auth.AuthResult
import com.jfleets.driver.data.auth.AuthService
import com.jfleets.driver.data.local.TokenManager
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class AuthRepository(
    private val authService: AuthService,
    private val tokenManager: TokenManager
) {
    fun getLoginIntent(): Intent {
        return authService.getAuthorizationIntent()
    }

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

    suspend fun refreshToken(): Result<Unit> = withContext(Dispatchers.IO) {
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

    suspend fun logout() = withContext(Dispatchers.IO) {
        tokenManager.clearTokens()
    }

    suspend fun isLoggedIn(): Boolean = withContext(Dispatchers.IO) {
        val token = tokenManager.getAccessToken()
        token != null && !tokenManager.isTokenExpired()
    }

    suspend fun isTokenExpired(): Boolean = withContext(Dispatchers.IO) {
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
