package com.logisticsx.driver.config

/**
 * Environment-based configuration. Platform implementations read from
 * Android BuildConfig (set at startup) or iOS Info.plist (set via xcconfig).
 */
expect object AppConfig {
    var apiBaseUrl: String
        private set
    var identityServerUrl: String
        private set
    var isProduction: Boolean
        private set
}

/**
 * Derived URLs that are the same formula on all platforms.
 */
val AppConfig.signalRHubUrl: String get() = "${apiBaseUrl}hubs/tracking"
val AppConfig.messagingHubUrl: String get() = "${apiBaseUrl}hubs/chat"
