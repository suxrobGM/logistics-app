package com.jfleets.driver.navigation

import androidx.compose.runtime.Composable
import androidx.navigation3.runtime.NavEntry
import androidx.navigation3.runtime.NavKey
import androidx.navigation3.runtime.entryProvider
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.api.TripApi
import com.jfleets.driver.api.models.InspectionType
import com.jfleets.driver.ui.screens.AboutScreen
import com.jfleets.driver.ui.screens.AccountScreen
import com.jfleets.driver.ui.screens.ConditionReportScreen
import com.jfleets.driver.ui.screens.ConversationScreen
import com.jfleets.driver.ui.screens.DashboardScreen
import com.jfleets.driver.ui.screens.DvirFormScreen
import com.jfleets.driver.ui.screens.EmployeeSelectScreen
import com.jfleets.driver.ui.screens.LoadDetailScreen
import com.jfleets.driver.ui.screens.LoginScreen
import com.jfleets.driver.ui.screens.MessagesScreen
import com.jfleets.driver.ui.screens.PastLoadsScreen
import com.jfleets.driver.ui.screens.PodCaptureScreen
import com.jfleets.driver.ui.screens.SettingsScreen
import com.jfleets.driver.ui.screens.StatsScreen
import com.jfleets.driver.ui.screens.TripDetailScreen
import com.jfleets.driver.ui.screens.TripsScreen
import com.jfleets.driver.viewmodel.DocumentCaptureType
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import com.jfleets.driver.viewmodel.TripDetailViewModel
import org.koin.compose.koinInject

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
        val tripApi: TripApi = koinInject()
        val viewModel = TripDetailViewModel(tripApi, key.tripId)

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
        val loadApi: LoadApi = koinInject()
        val driverApi: DriverApi = koinInject()
        val viewModel = LoadDetailViewModel(loadApi, driverApi, key.loadId)

        LoadDetailScreen(
            onNavigateBack = { navigator.goBack() },
            onOpenMaps = onOpenUrl,
            onCapturePod = { id ->
                navigator.navigate(PodCaptureRoute(id, "POD"))
            },
            onCaptureBol = { id ->
                navigator.navigate(PodCaptureRoute(id, "BOL"))
            },
            onPickupInspection = { id ->
                navigator.navigate(ConditionReportRoute(id, "Pickup"))
            },
            onDeliveryInspection = { id ->
                navigator.navigate(ConditionReportRoute(id, "Delivery"))
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
        val captureType = when (key.captureType) {
            "BOL" -> DocumentCaptureType.BOL
            else -> DocumentCaptureType.POD
        }

        PodCaptureScreen(
            loadId = key.loadId,
            tripStopId = key.tripStopId ?: "",
            captureType = captureType,
            onNavigateBack = { navigator.goBack() }
        )
    }

    // Condition Report Screen
    entry<ConditionReportRoute> { key ->
        val inspectionType = when (key.inspectionType) {
            "Delivery" -> InspectionType.DELIVERY
            else -> InspectionType.PICKUP
        }

        ConditionReportScreen(
            loadId = key.loadId,
            inspectionType = inspectionType,
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
