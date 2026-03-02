package com.logisticsx.driver

import com.logisticsx.driver.config.AppConfig
import com.logisticsx.driver.config.messagingHubUrl
import com.logisticsx.driver.config.signalRHubUrl
import com.logisticsx.driver.service.LocationService
import com.logisticsx.driver.service.IosNetworkMonitor
import com.logisticsx.driver.service.NetworkMonitor
import com.logisticsx.driver.service.auth.AuthService
import com.logisticsx.driver.service.createIosDataStore
import com.logisticsx.driver.service.messaging.MessagingService
import com.logisticsx.driver.service.realtime.SignalRService
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

private var koinInitialized = false

fun initKoin() {
    if (koinInitialized) {
        return
    }

    startKoin {
        modules(
            iosModule,
            commonModule()
        )
    }
    koinInitialized = true
}

/**
 * Koin module for iOS-specific dependencies
 */
val iosModule = module {
    single { createIosDataStore() }
    single { AuthService(AppConfig.identityServerUrl, get()) }
    single<SignalRService> { SignalRService(AppConfig.signalRHubUrl, get()) }
    single { MessagingService(AppConfig.messagingHubUrl, get()) }
    singleOf(::LocationService)
    single<NetworkMonitor> { IosNetworkMonitor() }
}
