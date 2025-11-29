package com.jfleets.driver.data.auth

/**
 * Multiplatform interface for handling OAuth login flow.
 * Platform-specific implementations handle the login UI differently:
 * - Android: Uses AppAuth with Intent-based flow and Activity Result
 * - iOS: Uses ASWebAuthenticationSession
 */
interface LoginService {
    /**
     * Initiates the login flow.
     * The implementation should handle the platform-specific OAuth flow
     * and call onResult with the outcome.
     *
     * @param onResult Callback with Result.success(Unit) on successful login,
     *                 or Result.failure(exception) on error
     */
    fun startLogin(onResult: (Result<Unit>) -> Unit)

    /**
     * Cancels any ongoing login flow.
     */
    fun cancelLogin()
}
