package com.jfleets.driver

import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.service.createIosDataStore
import com.jfleets.driver.service.realtime.SignalRService
import org.koin.core.context.startKoin
import org.koin.dsl.module

const val API_BASE_URL = "http://localhost:7000/"
const val IDENTITY_SERVER_URL = "http://localhost:7001/"
const val SIGNALR_HUB_URL = "http://localhost:7000/hubs/live-tracking"

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
    single { createIosDataStore() }
    single { AuthService(IDENTITY_SERVER_URL, get()) }
    single { SignalRService(SIGNALR_HUB_URL, get()) }
}
