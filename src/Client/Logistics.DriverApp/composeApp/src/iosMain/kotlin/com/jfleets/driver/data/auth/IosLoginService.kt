package com.jfleets.driver.data.auth

import com.jfleets.driver.data.local.PreferencesManager

/**
 * iOS implementation of LoginService.
 * TODO: Implement using ASWebAuthenticationSession for OAuth 2.0 flow.
 */
class IosLoginService(
    private val preferencesManager: PreferencesManager
) : LoginService {

    override fun startLogin(onResult: (Result<Unit>) -> Unit) {
        // TODO: Implement ASWebAuthenticationSession OAuth flow
        // For now, return a placeholder error
        onResult(Result.failure(NotImplementedError("iOS login not yet implemented")))
    }

    override fun cancelLogin() {
        // TODO: Cancel any ongoing ASWebAuthenticationSession
    }
}
