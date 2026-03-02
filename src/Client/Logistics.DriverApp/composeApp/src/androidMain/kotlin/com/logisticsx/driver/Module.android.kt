package com.logisticsx.driver

import android.content.Context
import androidx.activity.ComponentActivity
import com.logisticsx.driver.config.AppConfig
import com.logisticsx.driver.config.messagingHubUrl
import com.logisticsx.driver.config.signalRHubUrl
import com.logisticsx.driver.service.LocationService
import com.logisticsx.driver.service.AndroidNetworkMonitor
import com.logisticsx.driver.service.NetworkMonitor
import com.logisticsx.driver.service.auth.AuthService
import com.logisticsx.driver.service.createAndroidDataStore
import com.logisticsx.driver.service.messaging.MessagingService
import com.logisticsx.driver.service.realtime.SignalRService
import com.logisticsx.driver.util.BarcodeScannerLauncher
import com.logisticsx.driver.util.CameraLauncher
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

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
            commonModule()
        )
    }

    koinInitialized = true
}

private fun androidModule(
    cameraLauncher: CameraLauncher,
    barcodeScannerLauncher: BarcodeScannerLauncher
) = module {
    single { createAndroidDataStore(get<Context>()) }
    single { AuthService(AppConfig.identityServerUrl, get()) }
    single { SignalRService(AppConfig.signalRHubUrl, get()) }
    single { MessagingService(AppConfig.messagingHubUrl, get()) }
    singleOf(::LocationService)
    single<NetworkMonitor> { AndroidNetworkMonitor(get()) }

    // Platform-specific launchers
    single { cameraLauncher }
    single { barcodeScannerLauncher }
}
