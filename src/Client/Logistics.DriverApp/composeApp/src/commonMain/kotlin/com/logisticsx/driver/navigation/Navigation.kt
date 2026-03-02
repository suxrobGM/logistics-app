package com.logisticsx.driver.navigation

import androidx.compose.runtime.Composable
import androidx.navigation3.runtime.NavEntry
import androidx.navigation3.runtime.NavKey
import androidx.navigation3.runtime.entryProvider
import com.logisticsx.driver.api.models.InspectionType
import com.logisticsx.driver.ui.screens.AboutScreen
import com.logisticsx.driver.ui.screens.AccountScreen
import com.logisticsx.driver.ui.screens.ConditionReportScreen
import com.logisticsx.driver.ui.screens.ConversationScreen
import com.logisticsx.driver.ui.screens.DashboardScreen
import com.logisticsx.driver.ui.screens.DvirFormScreen
import com.logisticsx.driver.ui.screens.EmployeeSelectScreen
import com.logisticsx.driver.ui.screens.LoadDetailScreen
import com.logisticsx.driver.ui.screens.LoginScreen
import com.logisticsx.driver.ui.screens.MessagesScreen
import com.logisticsx.driver.ui.screens.PastLoadsScreen
import com.logisticsx.driver.ui.screens.PodCaptureScreen
import com.logisticsx.driver.ui.screens.SettingsScreen
import com.logisticsx.driver.ui.screens.StatsScreen
import com.logisticsx.driver.ui.screens.TripDetailScreen
import com.logisticsx.driver.ui.screens.TripsScreen
import com.logisticsx.driver.viewmodel.DocumentCaptureType
import com.logisticsx.driver.viewmodel.LoadDetailViewModel
import com.logisticsx.driver.viewmodel.TripDetailViewModel
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf

/**
 * Creates the entry provider that maps routes to screen composables.
 */
@Composable
fun createEntryProvider(
    navigator: Navigator,
    onOpenUrl: (String) -> Unit
): (NavKey) -> NavEntry<NavKey> = entryProvider {
    // Login Screen
    entry<LoginRoute> {
        LoginScreen(
            onLoginSuccess = {
                navigator.navigateAndClear(DashboardRoute, LoginRoute)
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
                navigator.navigate(DvirFormRoute(truckId = truckId))
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
                navigator.navigate(DvirFormRoute(tripId = tripId))
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
        PodCaptureScreen(
            loadId = key.loadId,
            tripStopId = key.tripStopId ?: "",
            captureType = key.captureType,
            onNavigateBack = { navigator.goBack() }
        )
    }

    // Condition Report Screen
    entry<ConditionReportRoute> { key ->
        ConditionReportScreen(
            loadId = key.loadId,
            inspectionType = key.inspectionType,
            onNavigateBack = { navigator.goBack() }
        )
    }

    // DVIR Form Screen
    entry<DvirFormRoute> { key ->
        DvirFormScreen(
            truckId = key.truckId,
            tripId = key.tripId,
            onNavigateBack = { navigator.goBack() }
        )
    }

    // Account Screen
    entry<AccountRoute> {
        AccountScreen(
            onNavigateToStats = { navigator.navigate(StatsRoute) },
            onNavigateToSettings = { navigator.navigate(SettingsRoute) }
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
}
