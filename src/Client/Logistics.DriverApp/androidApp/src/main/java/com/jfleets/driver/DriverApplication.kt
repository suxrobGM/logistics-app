package com.jfleets.driver

import android.app.Application
import com.google.firebase.FirebaseApp
import com.jfleets.driver.data.auth.AuthService
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.local.TokenManager
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.presentation.viewmodel.AccountViewModel
import com.jfleets.driver.presentation.viewmodel.DashboardViewModel
import com.jfleets.driver.presentation.viewmodel.LoadDetailViewModel
import com.jfleets.driver.presentation.viewmodel.LoginViewModel
import com.jfleets.driver.presentation.viewmodel.PastLoadsViewModel
import com.jfleets.driver.presentation.viewmodel.StatsViewModel
import com.jfleets.driver.shared.platformModule
import com.jfleets.driver.shared.sharedModule
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.module
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

/**
 * Koin module for Android-specific dependencies
 * This module provides Android-only services and components
 */
val androidModule = module {
    // Local Storage
    singleOf(::PreferencesManager)
    singleOf(::TokenManager)

    // Authentication (Android-specific)
    singleOf(::AuthService)
    singleOf(::AuthRepository)

    // ViewModels
    viewModelOf(::DashboardViewModel)
    viewModelOf(::AccountViewModel)
    viewModelOf(::LoadDetailViewModel)
    viewModelOf(::PastLoadsViewModel)
    viewModelOf(::StatsViewModel)
    viewModelOf(::LoginViewModel)
}
