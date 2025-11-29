package com.jfleets.driver.data.repository

import com.jfleets.driver.data.local.PreferencesManager

/**
 * iOS implementation of AuthRepository.
 * TODO: Implement using ASWebAuthenticationSession for OAuth 2.0 flow.
 */
class IosAuthRepository(
    private val preferencesManager: PreferencesManager
) : AuthRepository {

    override suspend fun isLoggedIn(): Boolean {
        return preferencesManager.getAccessToken() != null
    }

    override suspend fun isTokenExpired(): Boolean {
        val expiry = preferencesManager.getTokenExpiry() ?: return true
        return System.currentTimeMillis() > expiry
    }

    override suspend fun refreshToken(): Result<Unit> {
        // TODO: Implement token refresh for iOS
        return Result.failure(Exception("Not implemented"))
    }

    override suspend fun logout() {
        preferencesManager.clearAll()
    }
}
