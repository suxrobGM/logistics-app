package com.jfleets.driver

import com.jfleets.driver.data.local.IosPreferencesManager
import com.jfleets.driver.presentation.viewmodel.LoginViewModel
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.bind
import org.koin.dsl.module

private const val IDENTITY_SERVER_URL = "https://localhost:7001/"

/**
 * Koin module for iOS-specific dependencies
 */
val iosModule = module {
    // Local Storage - bind implementation to interface
    singleOf(::IosPreferencesManager) bind PreferencesManager::class

    // Auth Service (ROPC authentication)
    single { AuthService(IDENTITY_SERVER_URL, get()) }

    // ViewModels
    singleOf(::LoginViewModel)
}
