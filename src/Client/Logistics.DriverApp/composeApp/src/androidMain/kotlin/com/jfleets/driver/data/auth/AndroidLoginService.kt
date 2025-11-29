package com.jfleets.driver.data.auth

import android.app.Activity
import android.content.Intent
import androidx.activity.compose.ManagedActivityResultLauncher
import androidx.activity.result.ActivityResult
import com.jfleets.driver.data.local.TokenManager
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

/**
 * Android implementation of LoginService.
 * Uses AppAuth for OAuth 2.0 authentication with Intent-based flow.
 *
 * This service requires a registered ActivityResultLauncher to be set
 * before calling startLogin().
 */
class AndroidLoginService(
    private val authService: AuthService,
    private val tokenManager: TokenManager
) : LoginService {

    private var pendingCallback: ((Result<Unit>) -> Unit)? = null
    private var activityLauncher: ManagedActivityResultLauncher<Intent, ActivityResult>? = null

    /**
     * Sets the activity result launcher. Must be called from a Composable
     * before startLogin() is called.
     */
    fun setActivityLauncher(launcher: ManagedActivityResultLauncher<Intent, ActivityResult>) {
        this.activityLauncher = launcher
    }

    /**
     * Creates the authorization Intent for launching OAuth flow.
     */
    fun getAuthorizationIntent(): Intent {
        return authService.getAuthorizationIntent()
    }

    override fun startLogin(onResult: (Result<Unit>) -> Unit) {
        val launcher = activityLauncher
        if (launcher == null) {
            onResult(Result.failure(IllegalStateException("ActivityResultLauncher not registered")))
            return
        }

        pendingCallback = onResult
        val intent = authService.getAuthorizationIntent()
        launcher.launch(intent)
    }

    override fun cancelLogin() {
        pendingCallback = null
    }

    /**
     * Handles the OAuth callback from the activity result.
     * Called from the Composable's activity result handler.
     */
    fun handleActivityResult(result: ActivityResult) {
        val callback = pendingCallback ?: return
        pendingCallback = null

        if (result.resultCode == Activity.RESULT_OK && result.data != null) {
            CoroutineScope(Dispatchers.IO).launch {
                try {
                    val authResult = authService.handleAuthorizationResponse(result.data!!)
                    saveAuthResult(authResult)
                    callback(Result.success(Unit))
                } catch (e: Exception) {
                    callback(Result.failure(e))
                }
            }
        } else {
            callback(Result.failure(Exception("Login cancelled or failed")))
        }
    }

    private suspend fun saveAuthResult(authResult: AuthResult) {
        tokenManager.saveTokens(
            accessToken = authResult.accessToken,
            refreshToken = authResult.refreshToken,
            idToken = authResult.idToken,
            expiresIn = authResult.expiresIn
        )
    }
}
