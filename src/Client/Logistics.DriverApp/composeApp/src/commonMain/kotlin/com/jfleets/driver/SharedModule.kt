package com.jfleets.driver

import com.jfleets.driver.data.api.ApiClient
import com.jfleets.driver.data.api.DriverApi
import com.jfleets.driver.data.api.LoadApi
import com.jfleets.driver.data.api.StatsApi
import com.jfleets.driver.data.api.TruckApi
import com.jfleets.driver.data.api.UserApi
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

expect fun getTokenProvider(): TokenProvider

interface TokenProvider {
    suspend fun getAccessToken(): String?
    suspend fun getTenantId(): String?
    suspend fun getUserId(): String?
}

fun sharedModule(baseUrl: String) = module {
    // API Client
    single {
        val tokenProvider = getTokenProvider()
        ApiClient(
            baseUrl = baseUrl,
            getAccessToken = { tokenProvider.getAccessToken() },
            getTenantId = { tokenProvider.getTenantId() }
        )
    }

    // API Services
    singleOf(::LoadApi)
    singleOf(::TruckApi)
    singleOf(::UserApi)
    singleOf(::DriverApi)
    singleOf(::StatsApi)
}
