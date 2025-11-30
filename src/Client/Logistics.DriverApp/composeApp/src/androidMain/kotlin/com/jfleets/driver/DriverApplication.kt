package com.jfleets.driver

import android.app.Application
import com.google.firebase.FirebaseApp
import com.jfleets.driver.service.AndroidPreferencesManager
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.util.Logger
import com.jfleets.driver.viewmodel.AccountViewModel
import com.jfleets.driver.viewmodel.DashboardViewModel
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import com.jfleets.driver.viewmodel.LoginViewModel
import com.jfleets.driver.viewmodel.PastLoadsViewModel
import com.jfleets.driver.viewmodel.StatsViewModel
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.bind
import org.koin.dsl.module

class DriverApplication : Application() {
    companion object {
        private const val API_BASE_URL = "https://10.0.2.2:7000/"
        private const val IDENTITY_SERVER_URL = "https://10.0.2.2:7001/"
    }

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

        Logger.d("DriverApplication", "DriverApplication initialized")
    }

    /**
     * Koin module for Android-specific dependencies
     * This module provides Android-only services and components
     */
    private val androidModule = module {
        singleOf(::AndroidPreferencesManager) bind PreferencesManager::class
        single { AuthService(IDENTITY_SERVER_URL, get()) }

        // ViewModels
        viewModelOf(::DashboardViewModel)
        viewModelOf(::AccountViewModel)
        viewModelOf(::LoadDetailViewModel)
        viewModelOf(::PastLoadsViewModel)
        viewModelOf(::StatsViewModel)
        viewModelOf(::LoginViewModel)
    }
}
