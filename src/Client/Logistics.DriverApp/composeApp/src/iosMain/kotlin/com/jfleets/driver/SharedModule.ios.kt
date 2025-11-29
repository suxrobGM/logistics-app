package com.jfleets.driver

import com.jfleets.driver.data.auth.IosLoginService
import com.jfleets.driver.data.auth.LoginService
import com.jfleets.driver.data.local.IosPreferencesManager
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.data.repository.IosAuthRepository
import com.jfleets.driver.presentation.viewmodel.LoginViewModel
import org.koin.core.context.GlobalContext
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.bind
import org.koin.dsl.module

actual fun getTokenProvider(): TokenProvider {
    return object : TokenProvider {
        private val preferencesManager: PreferencesManager by lazy {
            GlobalContext.get().get()
        }

        override suspend fun getAccessToken(): String? {
            return preferencesManager.getAccessToken()
        }

        override suspend fun getTenantId(): String? {
            return preferencesManager.getTenantId()
        }

        override suspend fun getUserId(): String? {
            return preferencesManager.getUserId()
        }
    }
}

/**
 * Koin module for iOS-specific dependencies
 */
val iosModule = module {
    // Local Storage - bind implementation to interface
    singleOf(::IosPreferencesManager) bind PreferencesManager::class

    // Authentication - bind implementation to interface
    singleOf(::IosAuthRepository) bind AuthRepository::class

    // Login Service - bind implementation to interface
    singleOf(::IosLoginService) bind LoginService::class

    // ViewModels
    singleOf(::LoginViewModel)
}
