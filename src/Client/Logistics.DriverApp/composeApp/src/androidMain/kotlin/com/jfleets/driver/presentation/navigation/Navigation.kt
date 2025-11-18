package com.jfleets.driver.presentation.navigation

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.navArgument
import com.jfleets.driver.presentation.ui.screens.AboutScreen
import com.jfleets.driver.presentation.ui.screens.AccountScreen
import com.jfleets.driver.presentation.ui.screens.DashboardScreen
import com.jfleets.driver.presentation.ui.screens.LoadDetailScreen
import com.jfleets.driver.presentation.ui.screens.LoginScreen
import com.jfleets.driver.presentation.ui.screens.PastLoadsScreen
import com.jfleets.driver.presentation.ui.screens.SettingsScreen
import com.jfleets.driver.presentation.ui.screens.StatsScreen

@Composable
fun AppNavigation(
    navController: NavHostController,
    startDestination: String,
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
        ) {
            LoadDetailScreen(
                onNavigateBack = { navController.popBackStack() }
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
