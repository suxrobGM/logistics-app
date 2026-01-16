package com.jfleets.driver.ui

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.List
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material.icons.filled.BarChart
import androidx.compose.material.icons.filled.Dashboard
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Settings
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
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.navigation3.runtime.NavKey
import androidx.navigation3.ui.NavDisplay
import com.jfleets.driver.model.LocalUserSettings
import com.jfleets.driver.model.UserSettings
import com.jfleets.driver.navigation.AccountRoute
import com.jfleets.driver.navigation.ConversationRoute
import com.jfleets.driver.navigation.DashboardRoute
import com.jfleets.driver.navigation.LoadDetailRoute
import com.jfleets.driver.navigation.LoginRoute
import com.jfleets.driver.navigation.MessagesRoute
import com.jfleets.driver.navigation.Navigator
import com.jfleets.driver.navigation.PastLoadsRoute
import com.jfleets.driver.navigation.SettingsRoute
import com.jfleets.driver.navigation.StatsRoute
import com.jfleets.driver.navigation.createEntryProvider
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.ui.theme.LogisticsDriverTheme
import org.koin.compose.koinInject

/**
 * The main entry composable
 */
@Composable
fun DriverApp(onOpenUrl: (String) -> Unit) {
    val authService = koinInject<AuthService>()
    val preferencesManager = koinInject<PreferencesManager>()

    val userSettings by preferencesManager.getUserSettingsFlow().collectAsState(UserSettings())

    // Simple single back stack with Dashboard as start
    val backStack = remember { mutableStateListOf<NavKey>(DashboardRoute) }
    val navigator = remember { Navigator(backStack) }

    // Create the entry provider for all screens
    val entryProvider = createEntryProvider(navigator, onOpenUrl)

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

    val bottomNavItems = listOf(
        BottomNavItem("Dashboard", DashboardRoute, Icons.Default.Dashboard),
        BottomNavItem("Stats", StatsRoute, Icons.Default.BarChart),
        BottomNavItem("Messages", MessagesRoute, Icons.Default.Email),
        BottomNavItem("Past Loads", PastLoadsRoute, Icons.AutoMirrored.Filled.List),
        BottomNavItem("Account", AccountRoute, Icons.Default.AccountCircle),
        BottomNavItem("Settings", SettingsRoute, Icons.Default.Settings),
    )

    // Determine current destination for bottom bar visibility
    val currentDestination = navigator.currentDestination

    val showBottomBar = currentDestination != null &&
            currentDestination !is LoginRoute &&
            currentDestination !is LoadDetailRoute &&
            currentDestination !is ConversationRoute

    CompositionLocalProvider(LocalUserSettings provides userSettings) {
        LogisticsDriverTheme {
            Scaffold(
                bottomBar = {
                    if (!showBottomBar) {
                        return@Scaffold
                    }

                    NavigationBar {
                        bottomNavItems.forEach { item ->
                            val isSelected = currentDestination == item.route ||
                                    (currentDestination is LoadDetailRoute && item.route == DashboardRoute)

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
