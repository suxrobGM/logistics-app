package com.jfleets.driver

import android.content.Context
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.service.createAndroidDataStore
import com.jfleets.driver.viewmodel.AccountViewModel
import com.jfleets.driver.viewmodel.DashboardViewModel
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import com.jfleets.driver.viewmodel.LoginViewModel
import com.jfleets.driver.viewmodel.PastLoadsViewModel
import com.jfleets.driver.viewmodel.StatsViewModel
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.module

const val API_BASE_URL = "https://10.0.2.2:7000/"
const val IDENTITY_SERVER_URL = "https://10.0.2.2:7001/"

private var koinInitialized = false

fun initKoin(context: Context) {
    if (koinInitialized) {
        return
    }

    startKoin {
        androidLogger()
        androidContext(context)
        modules(
            // Android-specific module (must be loaded first to provide PreferencesManager)
            androidModule,
            commonModule(baseUrl = API_BASE_URL)
        )
    }

    koinInitialized = true
}

private val androidModule = module {
    // DataStore instance (platform-specific creation)
    single { createAndroidDataStore(get<Context>()) }

    single { AuthService(IDENTITY_SERVER_URL, get()) }

    // ViewModels
    viewModelOf(::DashboardViewModel)
    viewModelOf(::AccountViewModel)
    viewModelOf(::LoadDetailViewModel)
    viewModelOf(::PastLoadsViewModel)
    viewModelOf(::StatsViewModel)
    viewModelOf(::LoginViewModel)
}
