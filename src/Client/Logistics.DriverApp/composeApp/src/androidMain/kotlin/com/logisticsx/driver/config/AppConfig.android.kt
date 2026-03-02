package com.logisticsx.driver.config

/**
 * Android implementation of AppConfig.
 * Initialized from the app module's BuildConfig at startup via [initialize].
 */
actual object AppConfig {
    actual var apiBaseUrl: String = ""
        private set
    actual var identityServerUrl: String = ""
        private set
    actual var isProduction: Boolean = false
        private set

    fun initialize(
        apiBaseUrl: String,
        identityServerUrl: String,
        isProduction: Boolean
    ) {
        this.apiBaseUrl = apiBaseUrl
        this.identityServerUrl = identityServerUrl
        this.isProduction = isProduction
    }
}
