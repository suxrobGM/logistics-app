package com.jfleets.driver

import android.app.Application
import com.google.firebase.FirebaseApp
import com.jfleets.driver.di.androidModule
import com.jfleets.driver.shared.di.platformModule
import com.jfleets.driver.shared.di.sharedModule
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import timber.log.Timber

class DriverApplication : Application() {
    override fun onCreate() {
        super.onCreate()

        // Initialize Timber for logging
        if (BuildConfig.DEBUG) {
            Timber.plant(Timber.DebugTree())
        }

        // Initialize Koin for DI (KMP + Android)
        startKoin {
            androidLogger()
            androidContext(this@DriverApplication)
            modules(
                // Shared KMP modules
                sharedModule(baseUrl = "https://10.0.2.2:7000/"),
                platformModule(),
                // Android-specific module
                androidModule
            )
        }

        // Initialize Firebase
        FirebaseApp.initializeApp(this)

        Timber.d("DriverApplication initialized")
    }
}
