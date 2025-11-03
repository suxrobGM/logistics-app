package com.jfleets.driver.shared.di

import com.jfleets.driver.shared.data.api.*
import com.jfleets.driver.shared.data.repository.*
import com.jfleets.driver.shared.platform.PlatformSettings
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
    single { LoadApi(get()) }
    single { TruckApi(get()) }
    single { UserApi(get()) }
    single { DriverApi(get()) }
    single { StatsApi(get()) }

    // Repositories
    singleOf(::LoadRepository)
    singleOf(::TruckRepository)
    singleOf(::UserRepository)
    singleOf(::StatsRepository)
}

expect fun platformModule(): Module
