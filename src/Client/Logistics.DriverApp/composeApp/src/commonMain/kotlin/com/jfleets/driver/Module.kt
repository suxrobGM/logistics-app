package com.jfleets.driver

import com.jfleets.driver.api.ApiFactory
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.api.StatApi
import com.jfleets.driver.api.TruckApi
import com.jfleets.driver.api.UserApi
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.viewmodel.AccountViewModel
import com.jfleets.driver.viewmodel.DashboardViewModel
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import com.jfleets.driver.viewmodel.LoginViewModel
import com.jfleets.driver.viewmodel.PastLoadsViewModel
import com.jfleets.driver.viewmodel.SettingsViewModel
import com.jfleets.driver.viewmodel.StatsViewModel
import org.koin.core.module.dsl.singleOf
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.module

fun commonModule(baseUrl: String) = module {
    singleOf(::PreferencesManager)

    // Register ApiFactory as a singleton
    single { ApiFactory(baseUrl, get()) }

    // Generated API instances from ApiFactory
    single<LoadApi> { get<ApiFactory>().loadApi }
    single<TruckApi> { get<ApiFactory>().truckApi }
    single<UserApi> { get<ApiFactory>().userApi }
    single<DriverApi> { get<ApiFactory>().driverApi }
    single<StatApi> { get<ApiFactory>().statApi }

    viewModelOf(::DashboardViewModel)
    viewModelOf(::AccountViewModel)
    viewModelOf(::LoadDetailViewModel)
    viewModelOf(::PastLoadsViewModel)
    viewModelOf(::StatsViewModel)
    viewModelOf(::LoginViewModel)
    viewModelOf(::SettingsViewModel)
}
