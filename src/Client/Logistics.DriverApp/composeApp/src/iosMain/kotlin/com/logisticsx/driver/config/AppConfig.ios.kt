package com.logisticsx.driver.config

import platform.Foundation.NSBundle

/** iOS AppConfig, read from Info.plist (filled in from iosApp/Configuration/*.xcconfig). */
actual object AppConfig {
    actual var apiBaseUrl: String =
        NSBundle.mainBundle.objectForInfoDictionaryKey("API_BASE_URL") as? String
            ?: error("API_BASE_URL missing from Info.plist - set it in iosApp/Configuration/*.xcconfig")
        private set

    actual var identityServerUrl: String =
        NSBundle.mainBundle.objectForInfoDictionaryKey("IDENTITY_SERVER_URL") as? String
            ?: error("IDENTITY_SERVER_URL missing from Info.plist - set it in iosApp/Configuration/*.xcconfig")
        private set

    actual var isProduction: Boolean =
        (NSBundle.mainBundle.objectForInfoDictionaryKey("IS_PRODUCTION") as? String) == "YES"
        private set
}
