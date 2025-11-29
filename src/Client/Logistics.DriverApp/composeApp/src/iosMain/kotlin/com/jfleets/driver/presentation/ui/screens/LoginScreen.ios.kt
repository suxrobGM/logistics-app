package com.jfleets.driver.presentation.ui.screens

import androidx.compose.runtime.Composable

/**
 * iOS implementation: No-op since iOS uses ASWebAuthenticationSession directly.
 */
@Composable
actual fun LoginScreenEffect() {
    // No-op on iOS - ASWebAuthenticationSession doesn't require activity result handling
}

/**
 * iOS implementation: No-op since iOS handles back navigation differently.
 */
@Composable
actual fun LoginBackHandler() {
    // No-op on iOS - system handles back navigation
}
