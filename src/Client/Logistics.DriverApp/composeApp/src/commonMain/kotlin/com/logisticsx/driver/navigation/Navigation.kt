package com.logisticsx.driver.navigation

import androidx.navigation3.runtime.NavEntry
import androidx.navigation3.runtime.NavKey
import androidx.navigation3.runtime.entryProvider
import com.logisticsx.driver.api.models.DvirType
import com.logisticsx.driver.api.models.InspectionType
import com.logisticsx.driver.ui.screens.AboutScreen
import com.logisticsx.driver.ui.screens.AccountScreen
import com.logisticsx.driver.ui.screens.ConditionReportScreen
import com.logisticsx.driver.ui.screens.ConversationScreen
import com.logisticsx.driver.ui.screens.DashboardScreen
import com.logisticsx.driver.ui.screens.DvirFormScreen
import com.logisticsx.driver.ui.screens.EmployeeSelectScreen
import com.logisticsx.driver.ui.screens.LoadDetailScreen
import com.logisticsx.driver.ui.screens.LocationDisclosureScreen
import com.logisticsx.driver.ui.screens.LoginScreen
import com.logisticsx.driver.ui.screens.MessagesScreen
import com.logisticsx.driver.ui.screens.MyLicensesScreen
import com.logisticsx.driver.ui.screens.PastLoadsScreen
import com.logisticsx.driver.ui.screens.PodCaptureScreen
import com.logisticsx.driver.ui.screens.PrivacyScreen
import com.logisticsx.driver.ui.screens.SettingsScreen
import com.logisticsx.driver.ui.screens.StatsScreen
import com.logisticsx.driver.ui.screens.TripDetailScreen
import com.logisticsx.driver.ui.screens.TripsScreen
import com.logisticsx.driver.viewmodel.ConditionReportViewModel
import com.logisticsx.driver.viewmodel.DocumentCaptureType
import com.logisticsx.driver.viewmodel.DvirFormViewModel
import com.logisticsx.driver.viewmodel.LoadDetailViewModel
import com.logisticsx.driver.viewmodel.PodCaptureViewModel
import com.logisticsx.driver.viewmodel.TripDetailViewModel
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf

/**
 * Creates the entry provider that maps routes to screen composables.
 */
fun createEntryProvider(
    navigator: Navigator,
    onOpenUrl: (String) -> Unit
): (NavKey) -> NavEntry<NavKey> = entryProvider {
    // Login Screen
    entry<LoginRoute> {
        LoginScreen(
            onLoginSuccess = { hasAcceptedDisclosure ->
                if (hasAcceptedDisclosure) {
                    navigator.navigateAndClear(DashboardRoute, LoginRoute)
                } else {
                    navigator.navigateAndClear(LocationDisclosureRoute, LoginRoute)
                }
            }
        )
    }

    // Location Disclosure Screen (shown once before first dashboard load)
    entry<LocationDisclosureRoute> {
        LocationDisclosureScreen(
            onAccepted = {
                navigator.navigateAndClear(DashboardRoute, LocationDisclosureRoute)
            },
            onDeclined = {
                navigator.clearAndNavigate(LoginRoute)
            }
        )
    }

    // Dashboard Screen (Start destination after login)
    entry<DashboardRoute> {
        DashboardScreen(
            onLoadClick = { loadId ->
                navigator.navigate(LoadDetailRoute(loadId))
            },
            onTripClick = { tripId ->
                navigator.navigate(TripDetailRoute(tripId))
            },
            onDvirClick = { truckId ->
                navigator.navigate(DvirFormRoute(truckId = truckId, dvirType = DvirType.PRE_TRIP))
            },
            onLogout = {
                navigator.clearAndNavigate(LoginRoute)
            }
        )
    }

    // Stats Screen
    entry<StatsRoute> {
        StatsScreen()
    }

    // Past Loads Screen
    entry<PastLoadsRoute> {
        PastLoadsScreen(
            onLoadClick = { loadId ->
                navigator.navigate(LoadDetailRoute(loadId))
            }
        )
    }

    // Trips Screen
    entry<TripsRoute> {
        TripsScreen(
            onTripClick = { tripId ->
                navigator.navigate(TripDetailRoute(tripId))
            }
        )
    }

    // Trip Detail Screen
    entry<TripDetailRoute> { key ->
        val viewModel: TripDetailViewModel = koinViewModel { parametersOf(key.tripId) }

        TripDetailScreen(
            onNavigateBack = { navigator.goBack() },
            onOpenMaps = onOpenUrl,
            onLoadClick = { loadId ->
                navigator.navigate(LoadDetailRoute(loadId))
            },
            onDvirClick = { tripId ->
                navigator.navigate(DvirFormRoute(tripId = tripId, dvirType = DvirType.POST_TRIP))
            },
            viewModel = viewModel
        )
    }

    // Load Detail Screen
    entry<LoadDetailRoute> { key ->
        val viewModel: LoadDetailViewModel = koinViewModel { parametersOf(key.loadId) }

        LoadDetailScreen(
            onNavigateBack = { navigator.goBack() },
            onOpenMaps = onOpenUrl,
            onCapturePod = { id ->
                navigator.navigate(PodCaptureRoute(id, DocumentCaptureType.POD))
            },
            onCaptureBol = { id ->
                navigator.navigate(PodCaptureRoute(id, DocumentCaptureType.BOL))
            },
            onPickupInspection = { id ->
                navigator.navigate(ConditionReportRoute(id, InspectionType.PICKUP))
            },
            onDeliveryInspection = { id ->
                navigator.navigate(ConditionReportRoute(id, InspectionType.DELIVERY))
            },
            viewModel = viewModel
        )
    }

    // Messages Screen
    entry<MessagesRoute> {
        MessagesScreen(
            onConversationClick = { conversationId ->
                navigator.navigate(ConversationRoute(conversationId))
            },
            onNewMessage = {
                navigator.navigate(EmployeeSelectRoute)
            },
            onBack = { navigator.goBack() }
        )
    }

    // Employee Select Screen (New Message)
    entry<EmployeeSelectRoute> {
        EmployeeSelectScreen(
            onConversationCreated = { conversationId ->
                // Navigate to the conversation, replacing EmployeeSelectRoute
                navigator.navigateAndClear(ConversationRoute(conversationId), EmployeeSelectRoute)
            },
            onBack = { navigator.goBack() }
        )
    }

    // Conversation Screen
    entry<ConversationRoute> { key ->
        ConversationScreen(
            conversationId = key.conversationId,
            onBack = { navigator.goBack() }
        )
    }

    // POD Capture Screen
    entry<PodCaptureRoute> { key ->
        val viewModel: PodCaptureViewModel =
            koinViewModel { parametersOf(key.loadId, key.tripStopId ?: "", key.captureType) }

        PodCaptureScreen(
            onNavigateBack = { navigator.goBack() },
            viewModel = viewModel
        )
    }

    // Condition Report Screen
    entry<ConditionReportRoute> { key ->
        val viewModel: ConditionReportViewModel =
            koinViewModel { parametersOf(key.loadId, key.inspectionType) }

        ConditionReportScreen(
            onNavigateBack = { navigator.goBack() },
            viewModel = viewModel
        )
    }

    // DVIR Form Screen
    entry<DvirFormRoute> { key ->
        val viewModel: DvirFormViewModel =
            koinViewModel { parametersOf(key.truckId, key.tripId, key.dvirType) }

        DvirFormScreen(
            onNavigateBack = { navigator.goBack() },
            viewModel = viewModel
        )
    }

    // Account Screen
    entry<AccountRoute> {
        AccountScreen(
            onNavigateToStats = { navigator.navigate(StatsRoute) },
            onNavigateToSettings = { navigator.navigate(SettingsRoute) },
            onNavigateToPrivacy = { navigator.navigate(PrivacyRoute) },
            onNavigateToMyLicenses = { navigator.navigate(MyLicensesRoute) }
        )
    }

    // Settings Screen
    entry<SettingsRoute> {
        SettingsScreen()
    }

    // About Screen
    entry<AboutRoute> {
        AboutScreen()
    }

    // Privacy Screen
    entry<PrivacyRoute> {
        PrivacyScreen(onNavigateBack = { navigator.goBack() })
    }

    // My Licenses Screen
    entry<MyLicensesRoute> {
        MyLicensesScreen(onNavigateBack = { navigator.goBack() })
    }
}
