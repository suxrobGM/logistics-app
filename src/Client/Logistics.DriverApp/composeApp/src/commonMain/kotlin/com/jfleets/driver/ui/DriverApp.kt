package com.jfleets.driver.ui

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.List
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material.icons.filled.BarChart
import androidx.compose.material.icons.filled.Dashboard
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.ExperimentalMaterial3Api
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
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.jfleets.driver.model.LocalUserSettings
import com.jfleets.driver.model.UserSettings
import com.jfleets.driver.navigation.AppNavigation
import com.jfleets.driver.navigation.Screen
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.auth.AuthService
import com.jfleets.driver.ui.theme.LogisticsDriverTheme
import org.koin.compose.koinInject

/**
 * The main entry composable
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DriverApp(onOpenUrl: (String) -> Unit) {
    val authService = koinInject<AuthService>()
    val preferencesManager = koinInject<PreferencesManager>()

    val userSettings by preferencesManager.getUserSettingsFlow().collectAsState(UserSettings())
    val navController = rememberNavController()
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentDestination = navBackStackEntry?.destination

    // Check auth status once and navigate if logged in
    LaunchedEffect(Unit) {
        try {
            val isLoggedIn = authService.isLoggedIn()
            if (isLoggedIn) {
                navController.navigate(Screen.Dashboard.route) {
                    popUpTo(Screen.Login.route) { inclusive = true }
                }
            }
        } catch (_: Exception) {
            // Stay on login screen if check fails
        }
    }

    val bottomNavItems = listOf(
        BottomNavItem("Dashboard", Screen.Dashboard.route, Icons.Default.Dashboard),
        BottomNavItem("Stats", Screen.Stats.route, Icons.Default.BarChart),
        BottomNavItem("Messages", Screen.Messages.route, Icons.Default.Email),
        BottomNavItem("Past Loads", Screen.PastLoads.route, Icons.AutoMirrored.Filled.List),
        BottomNavItem("Account", Screen.Account.route, Icons.Default.AccountCircle),
        BottomNavItem("Settings", Screen.Settings.route, Icons.Default.Settings),
    )

    val showBottomBar = currentDestination?.route != Screen.Login.route &&
            !currentDestination?.route.orEmpty().startsWith("load_detail") &&
            !currentDestination?.route.orEmpty().startsWith("conversation")

    CompositionLocalProvider(LocalUserSettings provides userSettings) {
        LogisticsDriverTheme {
            Scaffold(
                bottomBar = {
                    if (!showBottomBar) {
                        return@Scaffold
                    }

                    NavigationBar {
                        bottomNavItems.forEach { item ->
                            NavigationBarItem(
                                icon = { Icon(item.icon, contentDescription = item.label) },
                                label = { Text(item.label) },
                                selected = currentDestination?.hierarchy?.any { it.route == item.route } == true,
                                onClick = {
                                    // Ignore navigation if already on the destination
                                    if (currentDestination?.route == item.route) {
                                        return@NavigationBarItem
                                    }

                                    navController.navigate(item.route) {
                                        popUpTo(navController.graph.findStartDestination().id) {
                                            saveState = true
                                        }
                                        launchSingleTop = true
                                        restoreState = true
                                    }
                                }
                            )
                        }
                    }
                }
            ) { innerPadding ->
                AppNavigation(
                    navController = navController,
                    startDestination = Screen.Login.route,
                    onOpenUrl = onOpenUrl,
                    modifier = Modifier.padding(innerPadding)
                )
            }
        }
    }
}

private data class BottomNavItem(
    val label: String,
    val route: String,
    val icon: ImageVector
)
