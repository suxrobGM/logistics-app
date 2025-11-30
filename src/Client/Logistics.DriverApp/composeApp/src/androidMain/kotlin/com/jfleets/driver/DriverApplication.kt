package com.jfleets.driver

import android.app.Application
import com.google.firebase.FirebaseApp
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin

const val API_BASE_URL = "https://10.0.2.2:7000/"

class DriverApplication : Application() {
    override fun onCreate() {
        super.onCreate()

        // Initialize Koin for DI (KMP + Android)
        startKoin {
            androidLogger()
            androidContext(this@DriverApplication)
            modules(
                // Android-specific module (must be loaded first to provide PreferencesManager)
                androidModule,
                commonModule(baseUrl = API_BASE_URL)
            )
        }

        // Initialize Firebase
        FirebaseApp.initializeApp(this)
    }
}
