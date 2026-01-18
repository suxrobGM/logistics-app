package com.jfleets.driver

import com.jfleets.driver.api.ApiFactory
import com.jfleets.driver.api.CustomerApi
import com.jfleets.driver.api.DocumentApi
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.EmployeeApi
import com.jfleets.driver.api.InspectionApi
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.api.MessageApi
import com.jfleets.driver.api.ReportApi
import com.jfleets.driver.api.StatApi
import com.jfleets.driver.api.TruckApi
import com.jfleets.driver.api.UserApi
import com.jfleets.driver.api.models.InspectionType
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.messaging.ConversationStateManager
import com.jfleets.driver.viewmodel.AccountViewModel
import com.jfleets.driver.viewmodel.ChatViewModel
import com.jfleets.driver.viewmodel.ConditionReportViewModel
import com.jfleets.driver.viewmodel.ConversationListViewModel
import com.jfleets.driver.viewmodel.DashboardViewModel
import com.jfleets.driver.viewmodel.DocumentCaptureType
import com.jfleets.driver.viewmodel.EmployeeSelectViewModel
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import com.jfleets.driver.viewmodel.LoginViewModel
import com.jfleets.driver.viewmodel.PastLoadsViewModel
import com.jfleets.driver.viewmodel.PodCaptureViewModel
import com.jfleets.driver.viewmodel.SettingsViewModel
import com.jfleets.driver.viewmodel.StatsViewModel
import io.ktor.client.HttpClient
import org.koin.core.module.dsl.singleOf
import org.koin.core.module.dsl.viewModel
import org.koin.core.module.dsl.viewModelOf
import org.koin.dsl.module

fun commonModule(baseUrl: String) = module {
    singleOf(::PreferencesManager)

    // Register ApiFactory as a singleton
    single { ApiFactory(baseUrl, get()) }

    // HttpClient for file upload operations
    single<HttpClient> { get<ApiFactory>().httpClient }

    // Generated API instances from ApiFactory
    single<CustomerApi> { get<ApiFactory>().customerApi }
    single<DocumentApi> { get<ApiFactory>().documentApi }
    single<DriverApi> { get<ApiFactory>().driverApi }
    single<EmployeeApi> { get<ApiFactory>().employeeApi }
    single<InspectionApi> { get<ApiFactory>().inspectionApi }
    single<LoadApi> { get<ApiFactory>().loadApi }
    single<MessageApi> { get<ApiFactory>().messageApi }
    single<ReportApi> { get<ApiFactory>().reportApi }
    single<StatApi> { get<ApiFactory>().statApi }
    single<TruckApi> { get<ApiFactory>().truckApi }
    single<UserApi> { get<ApiFactory>().userApi }

    // ConversationStateManager service for shared messaging state
    single { ConversationStateManager(get(), get()) }

    viewModelOf(::DashboardViewModel)
    viewModelOf(::AccountViewModel)
    viewModelOf(::LoadDetailViewModel)
    viewModelOf(::PastLoadsViewModel)
    viewModelOf(::StatsViewModel)
    viewModelOf(::LoginViewModel)
    viewModelOf(::SettingsViewModel)
    viewModelOf(::ConversationListViewModel)
    viewModelOf(::EmployeeSelectViewModel)

    // ChatViewModel with conversationId parameter
    viewModel { params ->
        ChatViewModel(
            messageApi = get(),
            preferencesManager = get(),
            messagingService = get(),
            conversationStateManager = get(),
            conversationId = params.get<String>()
        )
    }

    // PodCaptureViewModel with parameters
    viewModel { params ->
        PodCaptureViewModel(
            documentApi = get(),
            locationService = get(),
            loadId = params.get<String>(),
            tripStopId = params.getOrNull<String>(),
            captureType = params.get<DocumentCaptureType>()
        )
    }

    // ConditionReportViewModel with parameters
    viewModel { params ->
        ConditionReportViewModel(
            inspectionApi = get(),
            locationService = get(),
            loadId = params.get<String>(),
            inspectionType = params.get<InspectionType>()
        )
    }
}
