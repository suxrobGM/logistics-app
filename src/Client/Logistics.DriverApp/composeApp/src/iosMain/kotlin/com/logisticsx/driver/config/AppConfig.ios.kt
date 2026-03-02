package com.logisticsx.driver.config

import platform.Foundation.NSBundle

actual object AppConfig {
    actual var apiBaseUrl: String =
        NSBundle.mainBundle.objectForInfoDictionaryKey("API_BASE_URL") as? String
            ?: "http://localhost:7000/"
        private set

    actual var identityServerUrl: String =
        NSBundle.mainBundle.objectForInfoDictionaryKey("IDENTITY_SERVER_URL") as? String
            ?: "http://localhost:7001/"
        private set

    actual var isProduction: Boolean =
        (NSBundle.mainBundle.objectForInfoDictionaryKey("IS_PRODUCTION") as? String) == "YES"
        private set
}
