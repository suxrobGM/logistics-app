package com.jfleets.driver

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.List
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material.icons.filled.BarChart
import androidx.compose.material.icons.filled.Dashboard
import androidx.compose.material.icons.filled.Info
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.window.ComposeUIViewController
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.presentation.navigation.AppNavigation
import com.jfleets.driver.presentation.navigation.Screen
import com.jfleets.driver.presentation.ui.theme.LogisticsDriverTheme
import org.koin.compose.koinInject
import org.koin.core.context.startKoin
import platform.Foundation.NSURL
import platform.UIKit.UIApplication
import platform.UIKit.UIViewController

fun MainViewController(): UIViewController {
    initKoin()
    return ComposeUIViewController {
        DriverApp()
    }
}

private var koinInitialized = false

private fun initKoin() {
    if (!koinInitialized) {
        startKoin {
            modules(
                // iOS-specific module (must be loaded first to provide PreferencesManager)
                iosModule,
                // Shared KMP modules
                commonModule(baseUrl = "https://localhost:7000/")
            )
        }
        koinInitialized = true
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DriverApp() {
    val authRepository: AuthRepository = koinInject()
    val navController = rememberNavController()
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentDestination = navBackStackEntry?.destination

    // Callback to open URLs (for Maps, etc.)
    val onOpenUrl: (String) -> Unit = { url ->
        NSURL.URLWithString(url)?.let { nsUrl ->
            UIApplication.sharedApplication.openURL(nsUrl)
        }
    }

    // Check auth status once and navigate if logged in
    LaunchedEffect(Unit) {
        try {
            val isLoggedIn = authRepository.isLoggedIn()
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
        IosBottomNavItem("Dashboard", Screen.Dashboard.route, Icons.Default.Dashboard),
        IosBottomNavItem("Stats", Screen.Stats.route, Icons.Default.BarChart),
        IosBottomNavItem("Past Loads", Screen.PastLoads.route, Icons.AutoMirrored.Filled.List),
        IosBottomNavItem("Account", Screen.Account.route, Icons.Default.AccountCircle),
        IosBottomNavItem("Settings", Screen.Settings.route, Icons.Default.Settings),
        IosBottomNavItem("About", Screen.About.route, Icons.Default.Info)
    )

    val showBottomBar = currentDestination?.route != Screen.Login.route &&
            !currentDestination?.route.orEmpty().startsWith("load_detail")

    LogisticsDriverTheme {
        Scaffold(
            bottomBar = {
                if (showBottomBar) {
                    NavigationBar {
                        bottomNavItems.forEach { item ->
                            NavigationBarItem(
                                icon = { Icon(item.icon, contentDescription = item.label) },
                                label = { Text(item.label) },
                                selected = currentDestination?.hierarchy?.any { it.route == item.route } == true,
                                onClick = {
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

private data class IosBottomNavItem(
    val label: String,
    val route: String,
    val icon: ImageVector
)
