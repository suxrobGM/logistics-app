package com.jfleets.driver.di

import android.content.Context
import com.jfleets.driver.data.auth.AuthService
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.local.TokenManager
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.presentation.viewmodel.*
import org.koin.android.ext.koin.androidContext
import org.koin.androidx.viewmodel.dsl.viewModel
import org.koin.dsl.module

/**
 * Koin module for Android-specific dependencies
 * This module provides Android-only services and components
 */
val androidModule = module {
    // Android Context
    single<Context> { androidContext() }

    // Local Storage
    single { PreferencesManager(get()) }
    single { TokenManager(get()) }

    // Authentication (Android-specific)
    single { AuthService(get()) }
    single { AuthRepository(get(), get()) }

    // ViewModels
    viewModel { DashboardViewModel(get(), get(), get(), get()) }
    viewModel { AccountViewModel(get()) }
    viewModel { LoadDetailViewModel(get(), get()) } // LoadRepository + SavedStateHandle
    viewModel { PastLoadsViewModel(get()) }
    viewModel { StatsViewModel(get()) }
    viewModel { LoginViewModel(get()) }
}
