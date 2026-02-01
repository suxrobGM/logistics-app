package com.jfleets.driver.navigation

import androidx.navigation3.runtime.NavKey
import kotlinx.serialization.Serializable

/**
 * Navigation routes for the Driver app using Navigation 3.
 * Each route implements NavKey for type-safe navigation.
 */

@Serializable
data object LoginRoute : NavKey

@Serializable
data object DashboardRoute : NavKey

@Serializable
data object StatsRoute : NavKey

@Serializable
data object PastLoadsRoute : NavKey

@Serializable
data object TripsRoute : NavKey

@Serializable
data class TripDetailRoute(val tripId: String) : NavKey

@Serializable
data class LoadDetailRoute(val loadId: String) : NavKey

@Serializable
data object MessagesRoute : NavKey

@Serializable
data class ConversationRoute(val conversationId: String) : NavKey

@Serializable
data object EmployeeSelectRoute : NavKey

@Serializable
data class PodCaptureRoute(
    val loadId: String,
    val captureType: String,
    val tripStopId: String? = null
) : NavKey

@Serializable
data class ConditionReportRoute(
    val loadId: String,
    val inspectionType: String
) : NavKey

@Serializable
data class DvirFormRoute(
    val truckId: String? = null,
    val tripId: String? = null
) : NavKey

@Serializable
data object AccountRoute : NavKey

@Serializable
data object SettingsRoute : NavKey

@Serializable
data object AboutRoute : NavKey

/**
 * Top-level routes that appear in the bottom navigation bar.
 * These routes maintain separate back stacks.
 */
val topLevelRoutes: Set<NavKey> = setOf(
    DashboardRoute,
    TripsRoute,
    MessagesRoute,
    PastLoadsRoute,
    AccountRoute
)
