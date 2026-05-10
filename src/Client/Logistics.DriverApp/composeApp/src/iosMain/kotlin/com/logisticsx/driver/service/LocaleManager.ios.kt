package com.logisticsx.driver.service

import platform.Foundation.NSUserDefaults

class IosLocaleManager : LocaleManager {
    override fun apply(languageCode: String) {
        val tag = languageCode.ifBlank { "en" }
        NSUserDefaults.standardUserDefaults.setObject(listOf(tag), forKey = "AppleLanguages")
        NSUserDefaults.standardUserDefaults.synchronize()
        // The change becomes visible after the user backgrounds and relaunches the app —
        // restart UX lands in a follow-up alongside actual translated strings.
    }
}
