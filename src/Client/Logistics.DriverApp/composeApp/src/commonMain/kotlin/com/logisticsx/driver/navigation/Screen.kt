package com.logisticsx.driver.navigation

import androidx.navigation3.runtime.NavKey
import androidx.savedstate.serialization.SavedStateConfiguration
import com.logisticsx.driver.api.models.DvirType
import com.logisticsx.driver.api.models.InspectionType
import com.logisticsx.driver.viewmodel.DocumentCaptureType
import kotlinx.serialization.Serializable
import kotlinx.serialization.modules.SerializersModule
import kotlinx.serialization.modules.polymorphic
import kotlinx.serialization.modules.subclass

/**
 * Navigation routes for the Driver app using Navigation 3.
 * Each route implements NavKey for type-safe navigation.
 */

@Serializable
data object LoginRoute : NavKey

@Serializable
data object LocationDisclosureRoute : NavKey

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
    val captureType: DocumentCaptureType,
    val tripStopId: String? = null
) : NavKey

@Serializable
data class ConditionReportRoute(
    val loadId: String,
    val inspectionType: InspectionType
) : NavKey

@Serializable
data class DvirFormRoute(
    val truckId: String? = null,
    val tripId: String? = null,
    val dvirType: DvirType? = null
) : NavKey

@Serializable
data object AccountRoute : NavKey

@Serializable
data object SettingsRoute : NavKey

@Serializable
data object AboutRoute : NavKey

@Serializable
data object PrivacyRoute : NavKey

@Serializable
data object MyLicensesRoute : NavKey

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

/**
 * Saved-state configuration for [rememberNavBackStack][androidx.navigation3.runtime.rememberNavBackStack].
 *
 * On non-Android (iOS) targets there is no reflection-based overload, so every [NavKey] subtype
 * must be registered here for the back stack to survive process death / configuration changes.
 * Add new routes to this list when you add them above — an unregistered route crashes on save.
 */
val navSavedStateConfiguration: SavedStateConfiguration = SavedStateConfiguration {
    serializersModule = SerializersModule {
        polymorphic(NavKey::class) {
            subclass(LoginRoute::class)
            subclass(LocationDisclosureRoute::class)
            subclass(DashboardRoute::class)
            subclass(StatsRoute::class)
            subclass(PastLoadsRoute::class)
            subclass(TripsRoute::class)
            subclass(TripDetailRoute::class)
            subclass(LoadDetailRoute::class)
            subclass(MessagesRoute::class)
            subclass(ConversationRoute::class)
            subclass(EmployeeSelectRoute::class)
            subclass(PodCaptureRoute::class)
            subclass(ConditionReportRoute::class)
            subclass(DvirFormRoute::class)
            subclass(AccountRoute::class)
            subclass(SettingsRoute::class)
            subclass(AboutRoute::class)
            subclass(PrivacyRoute::class)
            subclass(MyLicensesRoute::class)
        }
    }
}
