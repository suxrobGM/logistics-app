package com.jfleets.driver.data.repository

import com.jfleets.driver.data.auth.LoginService
import com.jfleets.driver.data.local.PreferencesManager
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

/**
 * Android implementation of AuthRepository.
 * Uses the cross-platform LoginService for ROPC authentication.
 */
class AndroidAuthRepository(
    private val loginService: LoginService,
    private val preferencesManager: PreferencesManager
) : AuthRepository {

    override suspend fun refreshToken(): Result<Unit> = withContext(Dispatchers.IO) {
        loginService.refreshToken()
    }

    override suspend fun logout() = withContext(Dispatchers.IO) {
        loginService.logout()
    }

    override suspend fun isLoggedIn(): Boolean = withContext(Dispatchers.IO) {
        val token = preferencesManager.getAccessToken()
        token != null && !isTokenExpired()
    }

    override suspend fun isTokenExpired(): Boolean = withContext(Dispatchers.IO) {
        val expiry = preferencesManager.getTokenExpiry() ?: return@withContext true
        // Consider token expired 5 minutes before actual expiry
        System.currentTimeMillis() > (expiry - 300000)
    }
}
