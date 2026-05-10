package com.logisticsx.driver.service

import java.util.Locale

class AndroidLocaleManager : LocaleManager {
    override fun apply(languageCode: String) {
        val tag = languageCode.ifBlank { "en" }
        // Default-locale switch is enough for Compose resources to pick up the new
        // values-{lang}/ folder on next composition. Per-app system locale (Android 13+
        // AppCompatDelegate.setApplicationLocales) lands when actual translations roll in
        // and we add the AppCompat dependency.
        Locale.setDefault(Locale.forLanguageTag(tag))
    }
}
