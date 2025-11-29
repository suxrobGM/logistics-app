package com.jfleets.driver.data.repository

import com.jfleets.driver.data.auth.LoginService
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.util.currentTimeMillis

/**
 * iOS implementation of AuthRepository.
 * Uses the cross-platform LoginService for ROPC authentication.
 */
class IosAuthRepository(
    private val loginService: LoginService,
    private val preferencesManager: PreferencesManager
) : AuthRepository {

    override suspend fun isLoggedIn(): Boolean {
        val token = preferencesManager.getAccessToken()
        return token != null && !isTokenExpired()
    }

    override suspend fun isTokenExpired(): Boolean {
        val expiry = preferencesManager.getTokenExpiry() ?: return true
        // Consider token expired 5 minutes before actual expiry
        return currentTimeMillis() > (expiry - 300000)
    }

    override suspend fun refreshToken(): Result<Unit> {
        return loginService.refreshToken()
    }

    override suspend fun logout() {
        loginService.logout()
    }
}
