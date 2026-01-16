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
import com.jfleets.driver.api.models.InspectionType
import com.jfleets.driver.ui.screens.AboutScreen
import com.jfleets.driver.ui.screens.AccountScreen
import com.jfleets.driver.ui.screens.ConditionReportScreen
import com.jfleets.driver.ui.screens.ConversationScreen
import com.jfleets.driver.ui.screens.DashboardScreen
import com.jfleets.driver.ui.screens.LoadDetailScreen
import com.jfleets.driver.ui.screens.LoginScreen
import com.jfleets.driver.ui.screens.MessagesScreen
import com.jfleets.driver.ui.screens.PastLoadsScreen
import com.jfleets.driver.ui.screens.PodCaptureScreen
import com.jfleets.driver.ui.screens.SettingsScreen
import com.jfleets.driver.ui.screens.StatsScreen
import com.jfleets.driver.viewmodel.DocumentCaptureType
import com.jfleets.driver.viewmodel.LoadDetailViewModel
import org.koin.compose.koinInject

@Composable
fun AppNavigation(
    navController: NavHostController,
    startDestination: String,
    onOpenUrl: (String) -> Unit = {},
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
            // Extract loadId from route arguments
            val loadId = backStackEntry.savedStateHandle.get<String>("loadId") ?: ""
            val loadApi: LoadApi = koinInject()
            val driverApi: DriverApi = koinInject()
            val viewModel = LoadDetailViewModel(loadApi, driverApi, loadId)

            LoadDetailScreen(
                onNavigateBack = { navController.popBackStack() },
                onOpenMaps = onOpenUrl,
                onCapturePod = { id ->
                    navController.navigate(Screen.PodCapture.createRoute(id, "POD"))
                },
                onCaptureBol = { id ->
                    navController.navigate(Screen.PodCapture.createRoute(id, "BOL"))
                },
                onPickupInspection = { id ->
                    navController.navigate(Screen.ConditionReport.createRoute(id, "Pickup"))
                },
                onDeliveryInspection = { id ->
                    navController.navigate(Screen.ConditionReport.createRoute(id, "Delivery"))
                },
                viewModel = viewModel
            )
        }

        composable(Screen.Messages.route) {
            MessagesScreen(
                onConversationClick = { conversationId ->
                    navController.navigate(Screen.Conversation.createRoute(conversationId))
                },
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.Conversation.route,
            arguments = listOf(
                navArgument("conversationId") { type = NavType.StringType }
            )
        ) { backStackEntry ->
            val conversationId = backStackEntry.savedStateHandle.get<String>("conversationId") ?: ""
            ConversationScreen(
                conversationId = conversationId,
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.PodCapture.route,
            arguments = listOf(
                navArgument("loadId") { type = NavType.StringType },
                navArgument("captureType") { type = NavType.StringType },
                navArgument("tripStopId") {
                    type = NavType.StringType
                    nullable = true
                    defaultValue = null
                }
            )
        ) { backStackEntry ->
            val loadId = backStackEntry.savedStateHandle.get<String>("loadId") ?: ""
            val captureTypeStr = backStackEntry.savedStateHandle.get<String>("captureType") ?: "POD"
            val tripStopId = backStackEntry.savedStateHandle.get<String>("tripStopId") ?: ""
            val captureType = when (captureTypeStr) {
                "BOL" -> DocumentCaptureType.BOL
                else -> DocumentCaptureType.POD
            }

            PodCaptureScreen(
                loadId = loadId,
                tripStopId = tripStopId,
                captureType = captureType,
                onNavigateBack = { navController.popBackStack() },
                onCapturePhoto = {
                    // TODO: Launch camera - platform specific implementation
                }
            )
        }

        composable(
            route = Screen.ConditionReport.route,
            arguments = listOf(
                navArgument("loadId") { type = NavType.StringType },
                navArgument("inspectionType") { type = NavType.StringType }
            )
        ) { backStackEntry ->
            val loadId = backStackEntry.savedStateHandle.get<String>("loadId") ?: ""
            val inspectionTypeStr =
                backStackEntry.savedStateHandle.get<String>("inspectionType") ?: "Pickup"
            val inspectionType = when (inspectionTypeStr) {
                "Delivery" -> InspectionType.DELIVERY
                else -> InspectionType.PICKUP
            }

            ConditionReportScreen(
                loadId = loadId,
                inspectionType = inspectionType,
                onNavigateBack = { navController.popBackStack() },
                onCapturePhoto = {
                    // TODO: Launch camera - platform specific implementation
                },
                onScanVin = {
                    // TODO: Launch barcode scanner - platform specific implementation
                }
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
