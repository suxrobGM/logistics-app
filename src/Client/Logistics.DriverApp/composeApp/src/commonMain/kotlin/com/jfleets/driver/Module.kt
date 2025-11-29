package com.jfleets.driver

import com.jfleets.driver.data.api.ApiClient
import com.jfleets.driver.data.api.DriverApi
import com.jfleets.driver.data.api.LoadApi
import com.jfleets.driver.data.api.StatsApi
import com.jfleets.driver.data.api.TruckApi
import com.jfleets.driver.data.api.UserApi
import com.jfleets.driver.data.local.PreferencesManager
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

fun commonModule(baseUrl: String) = module {
    // API Client - uses PreferencesManager directly
    single {
        val preferencesManager: PreferencesManager = get()
        ApiClient(
            baseUrl = baseUrl,
            getAccessToken = { preferencesManager.getAccessToken() },
            getTenantId = { preferencesManager.getTenantId() }
        )
    }

    // API Services
    singleOf(::LoadApi)
    singleOf(::TruckApi)
    singleOf(::UserApi)
    singleOf(::DriverApi)
    singleOf(::StatsApi)
}
