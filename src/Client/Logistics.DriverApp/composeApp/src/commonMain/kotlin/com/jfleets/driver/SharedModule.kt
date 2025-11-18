package com.jfleets.driver

import com.jfleets.driver.data.api.ApiClient
import com.jfleets.driver.data.api.DriverApi
import com.jfleets.driver.data.api.LoadApi
import com.jfleets.driver.data.api.StatsApi
import com.jfleets.driver.data.api.TruckApi
import com.jfleets.driver.data.api.UserApi
import com.jfleets.driver.data.repository.LoadRepository
import com.jfleets.driver.data.repository.StatsRepository
import com.jfleets.driver.data.repository.TruckRepository
import com.jfleets.driver.data.repository.UserRepository
import com.jfleets.driver.platform.PlatformSettings
import org.koin.core.module.Module
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

fun sharedModule(baseUrl: String) = module {
    // API Client
    single {
        ApiClient(
            baseUrl = baseUrl,
            getAccessToken = { get<PlatformSettings>().getString("access_token") },
            getTenantId = { get<PlatformSettings>().getString("tenant_id") }
        )
    }

    // API Services
    singleOf(::LoadApi)
    singleOf(::TruckApi)
    singleOf(::UserApi)
    singleOf(::DriverApi)
    singleOf(::StatsApi)

    // Repositories
    singleOf(::LoadRepository)
    singleOf(::TruckRepository)
    singleOf(::UserRepository)
    singleOf(::StatsRepository)
}

expect fun platformModule(): Module
