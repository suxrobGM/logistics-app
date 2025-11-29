package com.jfleets.driver

import com.jfleets.driver.data.auth.LoginService
import com.jfleets.driver.data.local.IosPreferencesManager
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.data.repository.IosAuthRepository
import com.jfleets.driver.presentation.viewmodel.LoginViewModel
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

    // Login Service (ROPC authentication)
    single { LoginService(IDENTITY_SERVER_URL, get()) }

    // Auth Repository for token management
    singleOf(::IosAuthRepository) bind AuthRepository::class

    // ViewModels
    singleOf(::LoginViewModel)
}
