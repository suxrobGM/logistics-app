package com.jfleets.driver

import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.viewmodel.AccountViewModel
import com.jfleets.driver.viewmodel.DashboardViewModel
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import com.jfleets.driver.viewmodel.LoginViewModel
import com.jfleets.driver.viewmodel.PastLoadsViewModel
import com.jfleets.driver.viewmodel.StatsViewModel
import org.koin.core.module.dsl.singleOf
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.module

const val IDENTITY_SERVER_URL = "https://localhost:7001/"


val androidModule = module {
    singleOf(::PreferencesManager)
    single { AuthService(IDENTITY_SERVER_URL, get()) }

    // ViewModels
    viewModelOf(::DashboardViewModel)
    viewModelOf(::AccountViewModel)
    viewModelOf(::LoadDetailViewModel)
    viewModelOf(::PastLoadsViewModel)
    viewModelOf(::StatsViewModel)
    viewModelOf(::LoginViewModel)
}