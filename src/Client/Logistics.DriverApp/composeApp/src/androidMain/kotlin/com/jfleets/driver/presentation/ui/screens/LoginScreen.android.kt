package com.jfleets.driver.presentation.ui.screens

import android.app.Activity
import androidx.activity.compose.BackHandler
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.runtime.Composable
import androidx.compose.ui.platform.LocalContext
import com.jfleets.driver.data.auth.AndroidLoginService
import org.koin.compose.koinInject

/**
 * Android implementation: Registers ActivityResultLauncher for OAuth flow.
 */
@Composable
actual fun LoginScreenEffect() {
    val loginService: AndroidLoginService = koinInject()

    val loginLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.StartActivityForResult()
    ) { result ->
        loginService.handleActivityResult(result)
    }

    // Set the launcher on the service so it can be used when startLogin is called
    loginService.setActivityLauncher(loginLauncher)
}

/**
 * Android implementation: Handle back button to exit app from login screen.
 */
@Composable
actual fun LoginBackHandler() {
    val context = LocalContext.current
    BackHandler {
        (context as? Activity)?.finish()
    }
}
