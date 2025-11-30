package com.jfleets.driver

import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.viewmodel.LoginViewModel
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

private const val IDENTITY_SERVER_URL = "https://localhost:7001/"

/**
 * Koin module for iOS-specific dependencies
 */
val iosModule = module {
    singleOf(::PreferencesManager)

    // Auth Service (ROPC authentication)
    single { AuthService(IDENTITY_SERVER_URL, get()) }

    // ViewModels
    singleOf(::LoginViewModel)
}
