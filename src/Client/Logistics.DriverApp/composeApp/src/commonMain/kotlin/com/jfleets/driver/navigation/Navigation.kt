package com.jfleets.driver.navigation

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.navArgument
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.ui.screens.AboutScreen
import com.jfleets.driver.ui.screens.AccountScreen
import com.jfleets.driver.ui.screens.DashboardScreen
import com.jfleets.driver.ui.screens.LoadDetailScreen
import com.jfleets.driver.ui.screens.LoginScreen
import com.jfleets.driver.ui.screens.PastLoadsScreen
import com.jfleets.driver.ui.screens.SettingsScreen
import com.jfleets.driver.ui.screens.StatsScreen
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import org.koin.compose.koinInject

@Composable
fun AppNavigation(
    navController: NavHostController,
    startDestination: String,
    onOpenUrl: (String) -> Unit,
    modifier: Modifier = Modifier
) {
    NavHost(
        navController = navController,
        startDestination = startDestination,
        modifier = modifier
    ) {
        composable(Screen.Login.route) {
            LoginScreen(
                onLoginSuccess = {
                    navController.navigate(Screen.Dashboard.route) {
                        popUpTo(Screen.Login.route) { inclusive = true }
                    }
                }
            )
        }

        composable(Screen.Dashboard.route) {
            DashboardScreen(
                onLoadClick = { loadId ->
                    navController.navigate(Screen.LoadDetail.createRoute(loadId))
                },
                onLogout = {
                    navController.navigate(Screen.Login.route) {
                        popUpTo(navController.graph.id) {
                            inclusive = true
                        }
                        launchSingleTop = true
                    }
                }
            )
        }

        composable(Screen.Stats.route) {
            StatsScreen()
        }

        composable(Screen.PastLoads.route) {
            PastLoadsScreen(
                onLoadClick = { loadId ->
                    navController.navigate(Screen.LoadDetail.createRoute(loadId))
                }
            )
        }

        composable(
            route = Screen.LoadDetail.route,
            arguments = listOf(
                navArgument("loadId") { type = NavType.StringType }
            )
        ) { backStackEntry ->
            val loadId = backStackEntry.arguments?.getString("loadId") ?: ""
            val loadApi: LoadApi = koinInject()
            val driverApi: DriverApi = koinInject()
            val viewModel = LoadDetailViewModel(loadApi, driverApi, loadId)

            LoadDetailScreen(
                onNavigateBack = { navController.popBackStack() },
                onOpenMaps = onOpenUrl,
                viewModel = viewModel
            )
        }

        composable(Screen.Account.route) {
            AccountScreen()
        }

        composable(Screen.Settings.route) {
            SettingsScreen()
        }

        composable(Screen.About.route) {
            AboutScreen()
        }
    }
}
