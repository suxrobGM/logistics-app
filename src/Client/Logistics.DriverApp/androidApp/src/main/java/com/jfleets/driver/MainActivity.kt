package com.jfleets.driver

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.presentation.navigation.AppNavigation
import com.jfleets.driver.presentation.navigation.Screen
import com.jfleets.driver.presentation.ui.theme.LogisticsDriverTheme
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject

class MainActivity : ComponentActivity() {

    private val authRepository: AuthRepository by inject()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            LogisticsDriverTheme {
                val navController = rememberNavController()
                val scope = rememberCoroutineScope()
                var isLoggedIn by remember { mutableStateOf<Boolean?>(null) }

                // Check auth status
                LaunchedEffect(Unit) {
                    scope.launch {
                        isLoggedIn = authRepository.isLoggedIn()
                    }
                }

                // Show loading while checking auth
                if (isLoggedIn == null) {
                    Surface(color = MaterialTheme.colorScheme.background) {
                        // Loading state
                    }
                } else {
                    DriverAppScaffold(
                        startDestination = if (isLoggedIn == true) {
                            Screen.Dashboard.route
                        } else {
                            Screen.Login.route
                        }
                    )
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DriverAppScaffold(startDestination: String) {
    val navController = rememberNavController()
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentDestination = navBackStackEntry?.destination

    val bottomNavItems = listOf(
        BottomNavItem("Dashboard", Screen.Dashboard.route, Icons.Default.Dashboard),
        BottomNavItem("Stats", Screen.Stats.route, Icons.Default.BarChart),
        BottomNavItem("Past Loads", Screen.PastLoads.route, Icons.Default.List),
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
            startDestination = startDestination,
            modifier = Modifier.padding(innerPadding)
        )
    }
}

data class BottomNavItem(
    val label: String,
    val route: String,
    val icon: androidx.compose.ui.graphics.vector.ImageVector
)
