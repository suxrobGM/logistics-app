package com.jfleets.driver.data.repository

/**
 * Multiplatform interface for authentication operations.
 * Platform-specific implementations handle OAuth flows differently:
 * - Android: Uses AppAuth with Intent-based flow
 * - iOS: Uses ASWebAuthenticationSession
 */
interface AuthRepository {
    /**
     * Checks if the user is currently logged in with a valid token.
     */
    suspend fun isLoggedIn(): Boolean

    /**
     * Checks if the current access token has expired.
     */
    suspend fun isTokenExpired(): Boolean

    /**
     * Refreshes the access token using the stored refresh token.
     */
    suspend fun refreshToken(): Result<Unit>

    /**
     * Logs out the user by clearing all stored tokens.
     */
    suspend fun logout()
}
