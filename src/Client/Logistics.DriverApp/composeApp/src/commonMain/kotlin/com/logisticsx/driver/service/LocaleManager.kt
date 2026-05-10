package com.logisticsx.driver.service

/**
 * Applies a language change at the platform level so Compose Multiplatform
 * picks resources from the matching `composeResources/values-{lang}/` folder.
 * Implementations live in androidMain / iosMain and are wired via Koin.
 */
interface LocaleManager {
    fun apply(languageCode: String)
}
