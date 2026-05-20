package com.logisticsx.driver.ui

import androidx.compose.foundation.layout.WindowInsets
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.List
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material.icons.filled.Dashboard
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Route
import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.lifecycle.viewmodel.navigation3.rememberViewModelStoreNavEntryDecorator
import androidx.navigation3.runtime.NavKey
import androidx.navigation3.runtime.rememberNavBackStack
import androidx.navigation3.runtime.rememberSaveableStateHolderNavEntryDecorator
import androidx.navigation3.ui.NavDisplay
import com.logisticsx.driver.model.LocalUserSettings
import com.logisticsx.driver.model.UserSettings
import com.logisticsx.driver.navigation.AccountRoute
import com.logisticsx.driver.navigation.DashboardRoute
import com.logisticsx.driver.navigation.LoginRoute
import com.logisticsx.driver.navigation.MessagesRoute
import com.logisticsx.driver.navigation.Navigator
import com.logisticsx.driver.navigation.PastLoadsRoute
import com.logisticsx.driver.navigation.TripsRoute
import com.logisticsx.driver.navigation.createEntryProvider
import com.logisticsx.driver.navigation.navSavedStateConfiguration
import com.logisticsx.driver.navigation.topLevelRoutes
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.auth.AuthEvent
import com.logisticsx.driver.service.auth.AuthEventBus
import com.logisticsx.driver.service.auth.AuthService
import com.logisticsx.driver.ui.theme.LogisticsDriverTheme
import org.koin.compose.koinInject

/**
 * The main entry composable
 */
@Composable
fun DriverApp(onOpenUrl: (String) -> Unit) {
    val authService = koinInject<AuthService>()
    val preferencesManager = koinInject<PreferencesManager>()

    val userSettings by preferencesManager.getUserSettingsFlow().collectAsState(UserSettings())

    // Saveable single back stack with Dashboard as start (survives config change / process death)
    val backStack = rememberNavBackStack(navSavedStateConfiguration, DashboardRoute)
    val navigator = remember { Navigator(backStack) }

    // Create the entry provider for all screens (stable across recompositions)
    val entryProvider = remember(navigator, onOpenUrl) { createEntryProvider(navigator, onOpenUrl) }

    // Check auth status once and navigate if not logged in
    LaunchedEffect(Unit) {
        try {
            val isLoggedIn = authService.isLoggedIn()
            if (!isLoggedIn) {
                navigator.clearAndNavigate(LoginRoute)
            }
        } catch (_: Exception) {
            navigator.clearAndNavigate(LoginRoute)
        }
    }

    // Listen for 401 Unauthorized responses and redirect to login
    LaunchedEffect(Unit) {
        AuthEventBus.events.collect { event ->
            when (event) {
                is AuthEvent.Unauthorized -> {
                    authService.logout()
                    navigator.clearAndNavigate(LoginRoute)
                }
            }
        }
    }

    val bottomNavItems = listOf(
        BottomNavItem("Dashboard", DashboardRoute, Icons.Default.Dashboard),
        BottomNavItem("Trips", TripsRoute, Icons.Default.Route),
        BottomNavItem("Messages", MessagesRoute, Icons.Default.Email),
        BottomNavItem("Loads", PastLoadsRoute, Icons.AutoMirrored.Filled.List),
        BottomNavItem("Account", AccountRoute, Icons.Default.AccountCircle),
    )

    // Bottom bar shows only on top-level destinations.
    val currentDestination = navigator.currentDestination
    val showBottomBar = currentDestination in topLevelRoutes

    CompositionLocalProvider(LocalUserSettings provides userSettings) {
        LogisticsDriverTheme {
            Scaffold(
                contentWindowInsets = WindowInsets(0, 0, 0, 0),
                bottomBar = {
                    if (!showBottomBar) {
                        return@Scaffold
                    }

                    NavigationBar {
                        bottomNavItems.forEach { item ->
                            val isSelected = currentDestination == item.route

                            NavigationBarItem(
                                icon = { Icon(item.icon, contentDescription = item.label) },
                                label = { Text(item.label) },
                                selected = isSelected,
                                onClick = {
                                    if (!isSelected) {
                                        navigator.navigateToTopLevel(item.route)
                                    }
                                }
                            )
                        }
                    }
                }
            ) { innerPadding ->
                NavDisplay(
                    backStack = backStack,
                    entryProvider = entryProvider,
                    onBack = { navigator.goBack() },
                    // Scope a ViewModelStore to each NavEntry so parametrized ViewModels
                    // (load/trip/capture screens) are recreated per destination and cleared on pop.
                    entryDecorators = listOf(
                        rememberSaveableStateHolderNavEntryDecorator(),
                        rememberViewModelStoreNavEntryDecorator()
                    ),
                    modifier = Modifier.padding(innerPadding)
                )
            }
        }
    }
}

private data class BottomNavItem(
    val label: String,
    val route: NavKey,
    val icon: ImageVector
)
