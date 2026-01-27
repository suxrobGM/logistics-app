package com.jfleets.driver

import android.content.Context
import androidx.activity.ComponentActivity
import com.jfleets.driver.service.LocationService
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.service.createAndroidDataStore
import com.jfleets.driver.service.messaging.MessagingService
import com.jfleets.driver.service.realtime.SignalRService
import com.jfleets.driver.util.BarcodeScannerLauncher
import com.jfleets.driver.util.CameraLauncher
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

const val API_BASE_URL = "http://10.0.2.2:7000/"
const val IDENTITY_SERVER_URL = "http://10.0.2.2:7001/"
const val SIGNALR_HUB_URL = "http://10.0.2.2:7000/hubs/tracking"
const val MESSAGING_HUB_URL = "http://10.0.2.2:7000/hubs/chat"

private var koinInitialized = false

fun initKoin(activity: ComponentActivity) {
    if (koinInitialized) {
        return
    }

    // Create launchers before Koin initialization (must happen in onCreate before setContent)
    val cameraLauncher = CameraLauncher(activity)
    val barcodeScannerLauncher = BarcodeScannerLauncher(activity)

    startKoin {
        androidLogger()
        androidContext(activity)
        modules(
            // Android-specific module (must be loaded first to provide PreferencesManager)
            androidModule(cameraLauncher, barcodeScannerLauncher),
            commonModule(baseUrl = API_BASE_URL)
        )
    }

    koinInitialized = true
}

private fun androidModule(
    cameraLauncher: CameraLauncher,
    barcodeScannerLauncher: BarcodeScannerLauncher
) = module {
    single { createAndroidDataStore(get<Context>()) }
    single { AuthService(IDENTITY_SERVER_URL, get()) }
    single { SignalRService(SIGNALR_HUB_URL, get()) }
    single { MessagingService(MESSAGING_HUB_URL, get()) }
    singleOf(::LocationService)

    // Platform-specific launchers
    single { cameraLauncher }
    single { barcodeScannerLauncher }
}
