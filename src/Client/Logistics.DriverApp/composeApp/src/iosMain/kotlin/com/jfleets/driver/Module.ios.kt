package com.jfleets.driver

import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.service.createDataStore
import org.koin.core.context.startKoin
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
    // DataStore instance (platform-specific creation)
    single { createDataStore() }

    // Auth Service (ROPC authentication)
    single { AuthService(IDENTITY_SERVER_URL, get()) }
}
