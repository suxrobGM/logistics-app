package com.logisticsx.driver

import com.logisticsx.driver.api.ApiFactory
import com.logisticsx.driver.api.CustomerApi
import com.logisticsx.driver.config.AppConfig
import com.logisticsx.driver.api.DocumentApi
import com.logisticsx.driver.api.DriverApi
import com.logisticsx.driver.api.DvirApi
import com.logisticsx.driver.api.EmployeeApi
import com.logisticsx.driver.api.InspectionApi
import com.logisticsx.driver.api.LoadApi
import com.logisticsx.driver.api.MessageApi
import com.logisticsx.driver.api.ReportApi
import com.logisticsx.driver.api.StatApi
import com.logisticsx.driver.api.TripApi
import com.logisticsx.driver.api.TruckApi
import com.logisticsx.driver.api.UserApi
import com.logisticsx.driver.api.models.InspectionType
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.messaging.ConversationStateManager
import com.logisticsx.driver.viewmodel.AccountViewModel
import com.logisticsx.driver.viewmodel.ChatViewModel
import com.logisticsx.driver.viewmodel.ConditionReportViewModel
import com.logisticsx.driver.viewmodel.ConversationListViewModel
import com.logisticsx.driver.viewmodel.DashboardViewModel
import com.logisticsx.driver.viewmodel.DocumentCaptureType
import com.logisticsx.driver.viewmodel.DvirFormViewModel
import com.logisticsx.driver.viewmodel.EmployeeSelectViewModel
import com.logisticsx.driver.viewmodel.LoadDetailViewModel
import com.logisticsx.driver.viewmodel.LoginViewModel
import com.logisticsx.driver.viewmodel.PastLoadsViewModel
import com.logisticsx.driver.viewmodel.PodCaptureViewModel
import com.logisticsx.driver.viewmodel.SettingsViewModel
import com.logisticsx.driver.viewmodel.StatsViewModel
import com.logisticsx.driver.viewmodel.TripDetailViewModel
import com.logisticsx.driver.viewmodel.TripsViewModel
import io.ktor.client.HttpClient
import org.koin.core.module.dsl.singleOf
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.module

fun commonModule() = module {
    singleOf(::PreferencesManager)

    // Register ApiFactory as a singleton
    single { ApiFactory(AppConfig.apiBaseUrl, get(), get()) }

    // HttpClient for file upload operations
    single<HttpClient> { get<ApiFactory>().httpClient }

    // Generated API instances from ApiFactory
    single<CustomerApi> { get<ApiFactory>().customerApi }
    single<DocumentApi> { get<ApiFactory>().documentApi }
    single<DriverApi> { get<ApiFactory>().driverApi }
    single<DvirApi> { get<ApiFactory>().dvirApi }
    single<EmployeeApi> { get<ApiFactory>().employeeApi }
    single<InspectionApi> { get<ApiFactory>().inspectionApi }
    single<LoadApi> { get<ApiFactory>().loadApi }
    single<MessageApi> { get<ApiFactory>().messageApi }
    single<ReportApi> { get<ApiFactory>().reportApi }
    single<StatApi> { get<ApiFactory>().statApi }
    single<TripApi> { get<ApiFactory>().tripApi }
    single<TruckApi> { get<ApiFactory>().truckApi }
    single<UserApi> { get<ApiFactory>().userApi }

    // ConversationStateManager service for shared messaging state
    singleOf(::ConversationStateManager)

    viewModelOf(::DashboardViewModel)
    viewModelOf(::AccountViewModel)
    viewModelOf(::LoadDetailViewModel)
    viewModelOf(::PastLoadsViewModel)
    viewModelOf(::StatsViewModel)
    viewModelOf(::LoginViewModel)
    viewModelOf(::SettingsViewModel)
    viewModelOf(::ConversationListViewModel)
    viewModelOf(::EmployeeSelectViewModel)
    viewModelOf(::TripsViewModel)
    viewModelOf(::TripDetailViewModel)
    viewModelOf(::ChatViewModel)
    viewModelOf(::PodCaptureViewModel)
    viewModelOf(::ConditionReportViewModel)
    viewModelOf(::DvirFormViewModel)
}
