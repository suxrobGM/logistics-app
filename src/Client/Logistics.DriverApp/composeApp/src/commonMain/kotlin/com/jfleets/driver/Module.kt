package com.jfleets.driver

import com.jfleets.driver.api.ApiFactory
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.api.StatApi
import com.jfleets.driver.api.TruckApi
import com.jfleets.driver.api.UserApi
import org.koin.dsl.module

fun commonModule(baseUrl: String) = module {
    // Register ApiFactory as a singleton
    single { ApiFactory(baseUrl, get()) }

    // Generated API instances from ApiFactory
    single<LoadApi> { get<ApiFactory>().loadApi }
    single<TruckApi> { get<ApiFactory>().truckApi }
    single<UserApi> { get<ApiFactory>().userApi }
    single<DriverApi> { get<ApiFactory>().driverApi }
    single<StatApi> { get<ApiFactory>().statApi }
}
