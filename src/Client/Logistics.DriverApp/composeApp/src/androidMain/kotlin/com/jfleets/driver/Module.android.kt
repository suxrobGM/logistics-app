package com.jfleets.driver

import android.content.Context
import com.jfleets.driver.service.LocationService
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.service.createAndroidDataStore
import com.jfleets.driver.service.realtime.SignalRService
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

const val API_BASE_URL = "http://10.0.2.2:7000/"
const val IDENTITY_SERVER_URL = "http://10.0.2.2:7001/"
const val SIGNALR_HUB_URL = "http://10.0.2.2:7000/hubs/live-tracking"

private var koinInitialized = false

fun initKoin(context: Context) {
    if (koinInitialized) {
        return
    }

    startKoin {
        androidLogger()
        androidContext(context)
        modules(
            // Android-specific module (must be loaded first to provide PreferencesManager)
            androidModule,
            commonModule(baseUrl = API_BASE_URL)
        )
    }

    koinInitialized = true
}

private val androidModule = module {
    single { createAndroidDataStore(get<Context>()) }
    single { AuthService(IDENTITY_SERVER_URL, get()) }
    single { SignalRService(SIGNALR_HUB_URL, get()) }
    singleOf(::LocationService)
}
