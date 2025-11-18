package com.jfleets.driver

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
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
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.presentation.navigation.AppNavigation
import com.jfleets.driver.presentation.navigation.Screen
import com.jfleets.driver.presentation.ui.theme.LogisticsDriverTheme
import org.koin.compose.koinInject

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            LogisticsDriverTheme {
                DriverAppScaffold()
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DriverAppScaffold() {
    val authRepository: AuthRepository = koinInject()
    val navController = rememberNavController()
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentDestination = navBackStackEntry?.destination

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
        BottomNavItem("Dashboard", Screen.Dashboard.route, Icons.Default.Dashboard),
        BottomNavItem("Stats", Screen.Stats.route, Icons.Default.BarChart),
        BottomNavItem("Past Loads", Screen.PastLoads.route, Icons.AutoMirrored.Filled.List),
        BottomNavItem("Account", Screen.Account.route, Icons.Default.AccountCircle),
        BottomNavItem("Settings", Screen.Settings.route, Icons.Default.Settings),
        BottomNavItem("About", Screen.About.route, Icons.Default.Info)
    )

    val showBottomBar = currentDestination?.route != Screen.Login.route &&
            !currentDestination?.route.orEmpty().startsWith("load_detail")

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
                                    // Pop up to the start destination of the graph to
                                    // avoid building up a large stack of destinations
                                    popUpTo(navController.graph.findStartDestination().id) {
                                        saveState = true
                                    }
                                    // Avoid multiple copies of the same destination
                                    launchSingleTop = true
                                    // Restore state when reselecting a previously selected item
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
            modifier = Modifier.padding(innerPadding)
        )
    }
}

data class BottomNavItem(
    val label: String,
    val route: String,
    val icon: ImageVector
)
