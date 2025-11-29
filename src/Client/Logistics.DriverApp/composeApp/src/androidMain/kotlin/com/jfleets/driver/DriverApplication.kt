package com.jfleets.driver

import android.app.Application
import com.google.firebase.FirebaseApp
import com.jfleets.driver.data.auth.AndroidLoginService
import com.jfleets.driver.data.auth.AuthService
import com.jfleets.driver.data.auth.LoginService
import com.jfleets.driver.data.local.AndroidPreferencesManager
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.local.TokenManager
import com.jfleets.driver.data.repository.AndroidAuthRepository
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.presentation.viewmodel.AccountViewModel
import com.jfleets.driver.presentation.viewmodel.DashboardViewModel
import com.jfleets.driver.presentation.viewmodel.LoadDetailViewModel
import com.jfleets.driver.presentation.viewmodel.LoginViewModel
import com.jfleets.driver.presentation.viewmodel.PastLoadsViewModel
import com.jfleets.driver.presentation.viewmodel.StatsViewModel
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.singleOf
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.bind
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
                // Android-specific module (must be loaded first to provide PreferencesManager)
                androidModule,
                // Shared KMP modules
                commonModule(baseUrl = "https://10.0.2.2:7000/")
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
    // Local Storage - bind implementation to interface
    singleOf(::AndroidPreferencesManager) bind PreferencesManager::class
    singleOf(::TokenManager)

    // Authentication (Android-specific) - bind implementation to interface
    singleOf(::AuthService)
    singleOf(::AndroidAuthRepository) bind AuthRepository::class

    // Login Service (Android-specific) - bind implementation to interface
    singleOf(::AndroidLoginService) bind LoginService::class

    // ViewModels
    viewModelOf(::DashboardViewModel)
    viewModelOf(::AccountViewModel)
    viewModelOf(::LoadDetailViewModel)
    viewModelOf(::PastLoadsViewModel)
    viewModelOf(::StatsViewModel)
    viewModelOf(::LoginViewModel)
}
