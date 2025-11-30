package com.jfleets.driver

import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

const val API_BASE_URL = "https://localhost:7000/"
const val IDENTITY_SERVER_URL = "https://localhost:7001/"

private var koinInitialized = false

fun initKoin() {
    if (koinInitialized) {
        return
    }

    startKoin {
        modules(
            iosModule,
            commonModule(baseUrl = API_BASE_URL)
        )
    }
    koinInitialized = true
}

/**
 * Koin module for iOS-specific dependencies
 */
val iosModule = module {
    singleOf(::PreferencesManager)

    // Auth Service (ROPC authentication)
    single { AuthService(IDENTITY_SERVER_URL, get()) }
}
